using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("TemplatedQuery.EF")]
[assembly: InternalsVisibleTo("TemplatedQuery.EFCore")]


namespace NeuroSpeech.TemplatedQuery
{
    public struct Literal
    {
        public readonly string Value;

        public static Literal DoubleQuoted(string text) => new Literal($"\"{text}\"");

        public static Literal SquareBrackets(string text) => new Literal($"[{text}]");

        public Literal(string value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    public struct TemplateQuery
    {

        internal List<(string literal, bool hasArgument, object argument)> fragments;

        public static implicit operator TemplateQuery(FormattableString sql)
        {
            return new TemplateQuery(sql);
        }

        public TemplateQuery(FormattableString sql)
            : this(sql.Format, sql.GetArguments())
        {
        }

        public static TemplateQuery Join(string prefix, string separator, IEnumerable<TemplateQuery> fragments)
        {
            var r = prefix != null ? TemplateQuery.FromString(prefix) : TemplateQuery.New();
            var e = fragments.GetEnumerator();
            if (e.MoveNext())
            {
                r.fragments.AddRange(e.Current.fragments);
            }
            while (e.MoveNext())
            {
                r.fragments.Add((separator, false, null));
                r.fragments.AddRange(e.Current.fragments);
            }
            return r;
        }
        public static TemplateQuery Join(string separator, IEnumerable<TemplateQuery> fragments)
        {
            return Join(null, separator, fragments);
        }

        public static TemplateQuery Join(string prefix, string separator, params FormattableString[] fragments)
        {
            return TemplateQuery.Join(prefix, separator, fragments.Select(x => TemplateQuery.New(x)));
        }

        public static TemplateQuery Join(string separator, params FormattableString[] fragments)
        {
            return TemplateQuery.Join(null, separator, fragments);
        }

        private TemplateQuery(string text, object[] args)
        {
            fragments = new List<(string, bool, object)>();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            for (int i = 0; i < args.Length; i++)
            {
                var sep = $"{{{i}}}";
                var index = text.IndexOf(sep);

                var prefix = text.Substring(0, index);
                text = text.Substring(index + sep.Length);
                var arg = args[i];
                fragments.Add((prefix, false, null));
                if(arg is Literal l)
                {
                    fragments.Add((l.Value, false, null));
                } else if (arg is TemplateQuery q)
                {
                    fragments.AddRange(q.fragments);
                }
                else if (arg is TemplateFragments sf)
                {
                    var qf = sf.ToSqlQuery();
                    fragments.AddRange(qf.fragments);
                }
                else if (!(arg is string) && arg is System.Collections.IEnumerable en)
                {
                    var e = en.GetEnumerator();
                    if (e.MoveNext())
                    {
                        fragments.Add((null, true, e.Current));
                    }
                    while (e.MoveNext())
                    {
                        fragments.Add((",", false, null));
                        fragments.Add((null, true, e.Current));
                    }
                }
                else
                {
                    fragments.Add((null, true, args[i]));
                }
            }
            fragments.Add((text, false, null));

        }

        public FormattableString ToFormattableString()
        {
            return new SqlQueryFormattable(this);
        }

        public static TemplateQuery operator +(TemplateQuery first, FormattableString sql)
        {
            var r = new TemplateQuery(sql);
            r.fragments.InsertRange(0, first.fragments);
            return r;
        }

        public static TemplateQuery operator +(TemplateQuery first, TemplateQuery r)
        {
            r.fragments.InsertRange(0, first.fragments);
            return r;
        }

        public static TemplateQuery New()
        {
            return new TemplateQuery($"");
        }

        private static object[] Empty = new object[] { };        

        public static TemplateQuery Literal(string text)
        {
            return new TemplateQuery(text, Empty);
        }

        public static TemplateQuery New(params FormattableString[] sql)
        {
            if (sql.Length == 0)
            {
                throw new ArgumentException("Atleast one query must be specified");
            }
            var q = TemplateQuery.New();
            foreach (var s in sql)
            {
                q += s;
            }
            return q;
        }


        public override string ToString()
        {
            int ix = 0;
            return string.Join("", this.fragments.Select((x, i) => x.hasArgument ? $"{{{ix++}}}" : x.literal));
        }
        internal static TemplateQuery FromString(string format, params object[] parameters)
        {
            return new TemplateQuery(format, parameters);
        }

        public string Text
        {
            get
            {
                int ix = 0;
                return string.Join("", this.fragments.Select((x, i) => x.hasArgument ? $"@p{ix++}" : x.literal));
            }
        }

        public KeyValuePair<string, object>[] Values
        {
            get
            {
                int ix = 0;
                return this.fragments
                    .Where(x => x.hasArgument)
                    .Select(x => new KeyValuePair<string, object>(
                        $"p{ix++}",
                        x.argument))
                    .ToArray();
            }
        }

    }
}
