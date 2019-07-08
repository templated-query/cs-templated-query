using System;
using System.Linq;

namespace NeuroSpeech.TemplatedQuery
{
    internal class SqlQueryFormattable : FormattableString
    {
        object[] values;
        public SqlQueryFormattable(TemplateQuery query)
        {
            this.Format = query.ToString();
            this.values = query.fragments.Where(x => x.hasArgument).Select(x => x.argument).ToArray();
        }

        public override int ArgumentCount => values.Length;

        public override string Format { get; }

        public override object GetArgument(int index)
        {
            return values[index];
        }

        public override object[] GetArguments()
        {
            return values;
        }

        public override string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, Format, GetArguments());
        }
    }
}
