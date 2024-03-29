﻿using EM.Bases;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EM.SQLites
{
    /// <summary>
    /// 数据库扩展方法
    /// </summary>
    public static class SQLiteExtensions
    {
        /// <summary>
        /// 执行sql方法（自动打开数据库链接）
        /// </summary>
        /// <param name="connection">数据库链接</param>
        /// <param name="action">要执行的sql查询方法</param>
        /// <param name="useTransaction">使用事务</param>
        public static void Execute(this DbConnection connection, Action action, bool useTransaction = false)
        {
            if (connection == null || action == null)
            {
                return;
            }
            if (!LockContainer.TryGetValue(connection, out object lockObj))
            {
                lockObj = new object();
                LockContainer.TryAdd(connection, lockObj);
            }
            lock (lockObj)
            {
                try
                {
                    bool needClose = false;
                    if (connection.State != System.Data.ConnectionState.Open)
                    {
                        connection.Open();
                        needClose = true;
                    }
                    DbTransaction transaction = null;
                    if (useTransaction)
                    {
                        transaction = connection.BeginTransaction();
                    }

                    action.Invoke();//执行sql查询方法

                    if (transaction != null)
                    {
                        transaction.Commit();
                        transaction.Dispose();
                    }
                    if (needClose)
                    {
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"执行{nameof(Execute)}失败，{ex}");
                }
            }
        }
        /// <summary>
        /// 数据库连接锁容器
        /// </summary>
        private static ConcurrentDictionary<DbConnection, object> LockContainer { get; } = new ConcurrentDictionary<DbConnection, object>();

        /// <summary>
        /// 执行查询(自动打开)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="useTransaction">使用事务</param>
        /// <returns>受影响的行</returns>
        public static int ExecuteNonQueryWithAutoOpen(this DbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, bool useTransaction = false)
        {
            int ret = 0;
            if (connection == null || string.IsNullOrEmpty(sql))
            {
                return ret;
            }
            connection.Execute(() =>
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (parameters?.Count() > 0)
                    {
                        foreach (var item in parameters)
                        {
                            if (item != null)
                            {
                                cmd.Parameters.Add(item);
                            }
                        }
                    }
                    try
                    {
                        ret = cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"{nameof(ExecuteNonQueryWithAutoOpen)}失败,{sql},{e}");
                    }
                }
            }, useTransaction);
            return ret;
        }
        /// <summary>
        /// 执行查询(自动打开)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>受影响的行</returns>
        public static int ExecuteNonQuery(this DbConnection connection, string sql, IEnumerable<DbParameter> parameters = null)
        {
            int ret = 0;
            if (connection == null || string.IsNullOrEmpty(sql))
            {
                return ret;
            }
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                if (parameters?.Count() > 0)
                {
                    foreach (var item in parameters)
                    {
                        if (item != null)
                        {
                            cmd.Parameters.Add(item);
                        }
                    }
                }
                try
                {
                    ret = cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{nameof(ExecuteNonQueryWithAutoOpen)}失败,{sql},{e}");
                }
            }
            return ret;
        }
        /// <summary>
        /// 执行查询，并返回结果的第一行第一列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="useTransaction">使用事务</param>
        /// <returns>结果的第一行第一列</returns>
        public static object ExecuteScalar(this DbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, bool useTransaction = false)
        {
            object ret = null;
            if (string.IsNullOrEmpty(sql) || connection == null)
            {
                return ret;
            }
            connection.Execute(() =>
           {
               using (var cmd = connection.CreateCommand())
               {
                   cmd.CommandText = sql;
                   if (parameters?.Count() > 0)
                   {
                       foreach (var item in parameters)
                       {
                           cmd.Parameters.Add(item);
                       }
                   }
                   try
                   {
                       ret = cmd.ExecuteScalar();
                   }
                   catch (Exception e)
                   {
                       Debug.WriteLine($"{nameof(ExecuteScalar)}失败,{sql},{e}");
                   }
               }
           }, useTransaction);
            return ret;
        }

        /// <summary>
        /// 将类中带<seealso cref="FieldAttribute"/>的属性或字典转为列信息数组
        /// </summary>
        /// <typeparam name="dic">字典</typeparam>
        /// <returns>列信息数组</returns>
        public static TableInfo[] GetTableInfos(Dictionary<string, object> dic)
        {
            TableInfo[] ret = null;
            if (dic != null)
            {
                ret = new TableInfo[dic.Count];
                for (int i = 0; i < dic.Count; i++)
                {
                    var item = dic.ElementAt(i);
                    var type = item.Value.GetType();
                    TableInfo tableInfo = new TableInfo()
                    {
                        Name = item.Key
                    };
                    if (type == typeof(int) || type == typeof(uint))
                    {
                        tableInfo.Type = FieldType.INTEGER;
                    }
                    else if (type == typeof(double) || type == typeof(float))
                    {
                        tableInfo.Type = FieldType.REAL;
                    }
                    else if (type == typeof(byte[]))
                    {
                        tableInfo.Type = FieldType.BLOB;
                    }
                    else
                    {
                        tableInfo.Type = FieldType.TEXT;
                    }
                    ret[i] = tableInfo;
                }
            }
            return ret;
        }
        /// <summary>
        /// 将类中带<seealso cref="FieldAttribute"/>的属性转为列信息数组
        /// </summary>
        /// <param name="extraTableInfos">额外的字段信息</param>
        /// <returns>列信息数组</returns>
        public static TableInfo[] GetTableInfos<T>(IEnumerable<TableInfo> extraTableInfos = null)
        {
            Type type = typeof(T);
            var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<TableInfo> tableInfos = new List<TableInfo>();
            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.GetCustomAttribute(typeof(FieldAttribute)) is FieldAttribute fieldAttribute)
                {
                    tableInfos.Add(fieldAttribute.TableInfo);
                }
            }
            if (extraTableInfos != null)
            {
                foreach (var item in extraTableInfos)
                {
                    if (tableInfos.Any(x => x.Name == item.Name))
                    {
                        continue;
                    }
                    tableInfos.Add(item);
                }
            }
            return tableInfos.ToArray();
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="tableName">表名</param>
        /// <param name="fields">字段集合</param>
        /// <param name="filter">过滤条件</param>
        /// <returns>数据</returns>
        public static List<Dictionary<string, object>> GetFieldAndValuesList(this DbConnection connection, string tableName, IEnumerable<string> fields = null, string filter = null)
        {
            if (connection == null || string.IsNullOrEmpty(tableName))
            {
                return new List<Dictionary<string, object>>();
            }
            var sql = SQLiteQueries.GetSelectSql(tableName, fields, filter);
            var ret = connection.GetFieldAndValuesListBySql(sql);
            return ret;
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>数据集合</returns>
        public static List<Dictionary<string, object>> GetFieldAndValuesListBySql(this DbConnection connection, string sql, IEnumerable<DbParameter> parameters = null)
        {
            var ret = new List<Dictionary<string, object>>();
            if (connection == null || string.IsNullOrEmpty(sql))
            {
                return ret;
            }
            connection.Execute(() =>
           {
               using (var cmd = connection.CreateCommand())
               {
                   cmd.CommandText = sql;
                   if (parameters?.Count() > 0)
                   {
                       foreach (var item in parameters)
                       {
                           cmd.Parameters.Add(item);
                       }
                   }
                   using (var reader = cmd.ExecuteReader())
                   {
                       while (reader.Read())
                       {
                           var t = GetFieldAndValues(reader);
                           if (t != null)
                           {
                               ret.Add(t);
                           }
                       }
                       reader.Close();
                   }
               }
           });
            return ret;
        }
        /// <summary>
        /// 读取字段和值集合
        /// </summary>
        /// <param name="reader">数据读取器</param>
        /// <param name="fields">字段集合</param>
        /// <returns>字段和值集合</returns>
        public static Dictionary<string, object> GetFieldAndValues(DbDataReader reader, IEnumerable<string> fields = null)
        {
            var ret = new Dictionary<string, object>();
            if (reader == null)
            {
                return ret;
            }
            if (fields == null || fields.Count() == 0)
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var fieldName = reader.GetName(i);
                    var value = reader.GetValue(i);
                    ret.Add(fieldName, value);
                }
            }
            else
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var fieldName = reader.GetName(i);
                    if (fields.Contains(fieldName) && !ret.ContainsKey(fieldName))
                    {
                        var value = reader.GetValue(i);
                        ret.Add(fieldName, value);
                    }
                }
            }
            return ret;
        }
    }
}
