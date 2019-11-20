using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;

namespace NeuroSpeech.TemplatedQuery
{
    public static class DbContextExtensions
    {
        private static System.Data.Entity.Core.Objects.ObjectContext GetObjectContext(DbContext db)
        {
            return ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public static JArray FromSqlToJson(
            this DbContext db,
            TemplateQuery query)
        {
            return GetObjectContext(db).FromSqlToJson(query);
        }


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="db"></param>
        ///// <param name="format"></param>
        ///// <param name="parameters"></param>
        ///// <returns></returns>
        //internal static Task<JArray> FromSqlToJsonAsync(
        //    this DbContext db,
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
        public static Task<JArray> FromSqlToJsonAsync(
            this DbContext db,
            TemplateQuery query)
        {
            return GetObjectContext(db).FromSqlToJsonAsync(query);
        }

        public static Task<int> ExecuteNonQueryAsync(this DbContext db, TemplateQuery query)
        {
            return GetObjectContext(db).ExecuteNonQueryAsync(query);
        }

        public static int ExecuteNonQuery(this DbContext db, TemplateQuery query)
        {
            return GetObjectContext(db).ExecuteNonQuery(query);
        }


        public static List<T> FromSql<T>(
            this DbContext db,
            TemplateQuery[] queries,
            bool ignoreUnmatchedProperties = false)
            where T : class
        {
            return GetObjectContext(db).FromSql<T>(queries, ignoreUnmatchedProperties);
        }

        public static Task<List<T>> FromSqlAsync<T>(
            this DbContext db,
            TemplateQuery[] queries,
            bool ignoreUnmatchedProperties = false)
            where T : class
        {
            return GetObjectContext(db).FromSqlAsync<T>(queries, ignoreUnmatchedProperties);
        }

        public static List<T> FromSql<T>(
            this DbContext db,
            TemplateQuery query,
            bool ignoreUnmatchedProperties = false)
            where T : class
        {
            return GetObjectContext(db).FromSql<T>(query, ignoreUnmatchedProperties);
        }

        public static Task<List<T>> FromSqlAsync<T>(
            this DbContext db,
            TemplateQuery query,
            bool ignoreUnmatchedProperties = false)
            where T : class
        {
            return GetObjectContext(db).FromSqlAsync<T>(query, ignoreUnmatchedProperties);         
        }

    }
}

