using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.TemplatedQuery
{
    /// <summary>
    /// 
    /// </summary>
    public static class EFExtensions2
    {

        public class DbReader : IDisposable
        {
            private DbConnection conn;
            private DbCommand cmd;
            private DbDataReader reader;

            public DbDataReader Reader => reader;

            public DbCommand Command => cmd;

            public void Dispose()
            {
                try
                {
                    reader?.Dispose();
                }
                catch { }
                try
                {
                    cmd?.Dispose();
                }
                catch { }

                // Since EF will destroy connection, we should not
                //try
                //{
                //    conn?.Dispose();
                //}
                //catch { }
            }

            public static async Task<DbReader> CreateAsync(ObjectContext db, TemplateQuery query)
            {
                var r = await CreateCommandAsync(db, query);
                var cmd = r.cmd;
                r.reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                return r;
            }
            public static DbReader Create(ObjectContext db, TemplateQuery query)
            {
                var r = CreateCommand(db, query);
                var cmd = r.cmd;
                r.reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                return r;
            }

            public static DbReader CreateCommand(ObjectContext db, TemplateQuery query)
            {
                DbReader r = new DbReader();
                var conn = r.conn = (db.Connection as System.Data.Entity.Core.EntityClient.EntityConnection).StoreConnection ;
                if (conn.State != ConnectionState.Open)
                {
                    db.Connection.Open();
                }
                var cmd = r.cmd = conn.CreateCommand();
                string text = query.Text;

                //if (db.Database.IsMySql())
                //{
                //    text = text
                //        .Replace("[dbo].", "")
                //        .Replace("[", "`")
                //        .Replace("]", "`");
                //}
                cmd.CommandText = text;
                foreach (var kvp in query.Values)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = kvp.Key;
                    p.Value = kvp.Value ?? DBNull.Value;
                    cmd.Parameters.Add(p);
                }
                return r;
            }


            public static async Task<DbReader> CreateCommandAsync(ObjectContext db, TemplateQuery query)
            {
                DbReader r = new DbReader();
                var conn = r.conn = db.Connection;
                if (conn.State != ConnectionState.Open)
                {
                    await db.Connection.OpenAsync();
                }
                var cmd = r.cmd = conn.CreateCommand();
                string text = query.Text;

                //if (db.Database.IsMySql())
                //{
                //    text = text
                //        .Replace("[dbo].", "")
                //        .Replace("[", "`")
                //        .Replace("]", "`");
                //}
                cmd.CommandText = text;
                foreach (var kvp in query.Values)
                {
                    var p = cmd.CreateParameter();
                    p.ParameterName = kvp.Key;
                    p.Value = kvp.Value ?? DBNull.Value;
                    cmd.Parameters.Add(p);
                }
                return r;
            }

        }

        //internal static Task<List<T>> FromSqlAsync<T>(this ObjectContext db, string format, params object[] parameters)
        //    where T : class
        //{
        //    return FromSqlAsync<T>(db, TemplateQuery.FromString(format, parameters));
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="db"></param>
        ///// <param name="format"></param>
        ///// <param name="parameters"></param>
        ///// <returns></returns>
        //internal static JArray FromSqlToJson(
        //    this ObjectContext db,
        //    string format,
        //    params object[] parameters)
        //{
        //    return FromSqlToJson(db, TemplateQuery.FromString(format, parameters));
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static JArray FromSqlToJson(
            this ObjectContext db,
            TemplateQuery query)
        {
            JArray list = new JArray();
            var props = new List<(int index, string name)>();
            using (var dbReader = DbReader.Create(db, query))
            {
                var reader = dbReader.Reader;
                while (reader.Read())
                {
                    if (props.Count == 0)
                    {
                        if (reader.FieldCount == 0)
                        {
                            return list;
                        }
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var n = reader.GetName(i);
                            props.Add((i, n));
                        }

                    }

                    var item = new JObject();

                    foreach (var (index, n) in props)
                    {
                        var value = reader.GetValue(index);
                        if (value == null || value == DBNull.Value)
                        {
                            continue;
                        }
                        item.Add(n, JToken.FromObject(value));
                    }
                    list.Add(item);
                }
                return list;
            }
        }


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="db"></param>
        ///// <param name="format"></param>
        ///// <param name="parameters"></param>
        ///// <returns></returns>
        //internal static Task<JArray> FromSqlToJsonAsync(
        //    this ObjectContext db,
        //    string format,
        //    params object[] parameters)
        //{
        //    return FromSqlToJsonAsync(db, TemplateQuery.FromString(format, parameters));
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static async Task<JArray> FromSqlToJsonAsync(
            this ObjectContext db,
            TemplateQuery query)
        {
            JArray list = new JArray();
            var props = new List<(int index, string name)>();
            using (var dbReader = await DbReader.CreateAsync(db, query))
            {
                var reader = dbReader.Reader;
                while (await reader.ReadAsync())
                {
                    if (props.Count == 0)
                    {
                        if (reader.FieldCount == 0)
                        {
                            return list;
                        }
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var n = reader.GetName(i);
                            props.Add((i, n));
                        }

                    }

                    var item = new JObject();

                    foreach (var (index, n) in props)
                    {
                        var value = reader.GetValue(index);
                        if (value == null || value == DBNull.Value)
                        {
                            continue;
                        }
                        item.Add(n, JToken.FromObject(value));
                    }
                    list.Add(item);
                }
                return list;
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(this ObjectContext db, TemplateQuery query)
        {
            using (var r = await DbReader.CreateCommandAsync(db, query))
            {
                return await r.Command.ExecuteNonQueryAsync();
            }
        }

        public static int ExecuteNonQuery(this ObjectContext db, TemplateQuery query)
        {
            using (var r = DbReader.CreateCommand(db, query))
            {
                return r.Command.ExecuteNonQuery();
            }
        }

        private static ConcurrentDictionary<string, PropertyInfo> propertyCache
            = new ConcurrentDictionary<string, PropertyInfo>();

        public static List<T> FromSql<T>(
    this ObjectContext db,
    TemplateQuery[] queries,
    bool ignoreUnmatchedProperties = false)
    where T : class
        {
            if (queries == null || queries.Length == 0)
                throw new ArgumentException($"No query specified");
            TemplateQuery q = (FormattableString)$"";
            for (int i = 0; i < queries.Length; i++)
            {
                q = queries[i];
                if (i == queries.Length - 1)
                {
                    break;
                }
                db.ExecuteNonQuery(q);
            }
            return db.FromSql<T>(q, ignoreUnmatchedProperties);
        }

        public static async Task<List<T>> FromSqlAsync<T>(
            this ObjectContext db,
            TemplateQuery[] queries,
            bool ignoreUnmatchedProperties = false)
            where T : class
        {
            if (queries == null || queries.Length == 0)
                throw new ArgumentException($"No query specified");
            TemplateQuery q = (FormattableString)$"";
            for (int i = 0; i < queries.Length; i++)
            {
                q = queries[i];
                if (i == queries.Length - 1)
                {
                    break;
                }
                await db.ExecuteNonQueryAsync(q);
            }
            return await db.FromSqlAsync<T>(q, ignoreUnmatchedProperties);
        }

        public static List<T> FromSql<T>(
            this ObjectContext db,
            TemplateQuery query,
            bool ignoreUnmatchedProperties = false)
            where T : class
        {
            List<T> list = new List<T>();
            var props = new List<(int index, PropertyInfo property, string name)>();
            using (var dbReader = DbReader.Create(db, query))
            {
                var reader = dbReader.Reader;
                while (reader.Read())
                {
                    if (props.Count == 0)
                    {
                        if (reader.FieldCount == 0)
                        {
                            return list;
                        }
                        Type type = typeof(T);
                        List<string> notFound = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var n = reader.GetName(i);
                            var key = $"{type.FullName}.{n}";
                            var p = propertyCache.GetOrAdd(key, a => type.GetProperty(n, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public));
                            props.Add((i, p, n));
                        }

                        var empty = props.Where(x => x.property == null).Select(x => x.name);
                        if (empty.Any())
                        {
                            if (!ignoreUnmatchedProperties)
                            {
                                throw new InvalidOperationException($"Properties {string.Join(",", empty)} not found in {type.FullName}");
                            }
                            props = props.Where(x => x.property != null).ToList();
                        }

                    }

                    var item = Activator.CreateInstance<T>();

                    foreach (var (index, property, n) in props)
                    {
                        var value = reader.GetValue(index);
                        if (value == null || value == DBNull.Value)
                        {
                            continue;
                        }
                        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        if (value.GetType() != type)
                        {
                            value = Convert.ChangeType(value, type);
                        }

                        property.SetValue(item, value);

                    }
                    list.Add(item);
                }
                return list;
            }
        }

        public static async Task<List<T>> FromSqlAsync<T>(
            this ObjectContext db,
            TemplateQuery query,
            bool ignoreUnmatchedProperties = false)
            where T : class
        {
            List<T> list = new List<T>();
            var props = new List<(int index, PropertyInfo property, string name)>();
            using (var dbReader = await DbReader.CreateAsync(db, query))
            {
                var reader = dbReader.Reader;
                while (await reader.ReadAsync())
                {
                    if (props.Count == 0)
                    {
                        if (reader.FieldCount == 0)
                        {
                            return list;
                        }
                        Type type = typeof(T);
                        List<string> notFound = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var n = reader.GetName(i);
                            var key = $"{type.FullName}.{n}";
                            var p = propertyCache.GetOrAdd(key, a => type.GetProperty(n, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public));
                            props.Add((i, p, n));
                        }

                        var empty = props.Where(x => x.property == null).Select(x => x.name);
                        if (empty.Any())
                        {
                            if (!ignoreUnmatchedProperties)
                            {
                                throw new InvalidOperationException($"Properties {string.Join(",", empty)} not found in {type.FullName}");
                            }
                            props = props.Where(x => x.property != null).ToList();
                        }

                    }

                    var item = Activator.CreateInstance<T>();

                    foreach (var (index, property, n) in props)
                    {
                        var value = reader.GetValue(index);
                        if (value == null || value == DBNull.Value)
                        {
                            continue;
                        }
                        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                        if (value.GetType() != type)
                        {
                            value = Convert.ChangeType(value, type);
                        }

                        property.SetValue(item, value);

                    }
                    list.Add(item);
                }
                return list;
            }
        }

    }
}
