using EM.Bases;
using System;
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
        /// <summary>
        /// 执行sql方法（自动打开数据库链接）
        /// </summary>
        /// <param name="connection">数据库链接</param>
        /// <param name="func">要执行的sql查询方法</param>
        /// <param name="useTransaction">使用事务</param>
        /// <returns>任务</returns>
        public static async Task ExecuteAsync(this DbConnection connection, Func<Task> func, bool useTransaction = false)
        {
            if (connection == null || func == null)
            {
                return;
            }
            bool needClose = false;
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
                needClose = true;
            }
            DbTransaction transaction = null;
            if (useTransaction)
            {
                transaction = connection.BeginTransaction();
            }
            try
            {
                await func.Invoke();//执行sql查询方法
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(ExecuteAsync)}失败，{e}");
            }

            if (transaction != null)
            {
                transaction.Dispose();
            }
            if (needClose)
            {
                connection.Close();
            }
        }
        /// <summary>
        /// 执行sql方法（自动打开数据库链接）
        /// </summary>
        /// <param name="connection">数据库链接</param>
        /// <param name="action">要执行的sql查询方法</param>
        /// <param name="useTransaction">使用事务</param>
        /// <returns>任务</returns>
        public static async Task ExecuteAsync(this DbConnection connection, Action action, bool useTransaction = false)
        {
            if (connection == null || action == null)
            {
                return;
            }
            bool needClose = false;
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
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
        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="useTransaction">使用事务</param>
        /// <returns>受影响的行</returns>
        public static async Task<int> ExecuteNonQueryAsync(this DbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, bool useTransaction = false)
        {
            int ret = 0;
            if (string.IsNullOrEmpty(sql) || connection == null)
            {
                return ret;
            }
            await connection.ExecuteAsync(async () =>
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
                    ret = await cmd.ExecuteNonQueryAsync();
                }
            }, useTransaction);
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
        public static async Task<object> ExecuteScalarAsync(this DbConnection connection, string sql, IEnumerable<DbParameter> parameters = null, bool useTransaction = false)
        {
            object ret = null;
            if (string.IsNullOrEmpty(sql) || connection == null)
            {
                return ret;
            }
            await connection.ExecuteAsync(async () =>
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
                    ret = await cmd.ExecuteScalarAsync();
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
        public static TableInfo[] GetTableInfos<T>(IEnumerable<TableInfo> extraTableInfos=null)
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
            if (extraTableInfos!=null)
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
        /// 强制设置属性的值(数字之间可转换)
        /// </summary>
        /// <param name="propertyInfo">属性信息</param>
        /// <param name="destObj">目标对象</param>
        /// <param name="value">值</param>
        public static void ForceSetValue(this PropertyInfo propertyInfo, object destObj, object value)
        {
            if (propertyInfo == null || DBNull.Value.Equals(value) || value == null || destObj == null)
            {
                return;
            }
            var fieldType = value.GetType();
            var destType = propertyInfo.PropertyType;
            if (destType.Equals(fieldType))
            {
                propertyInfo.SetValue(destObj, value);
            }
            else
            {
                if (destType.IsEnum)
                {
                    var destValue = Enum.ToObject(destType, value);
                    propertyInfo.SetValue(destObj, destValue);
                }
                else
                {
                    bool isSuccess;
                    switch (destType.Name)
                    {
                        case "Int32":
                            isSuccess = value.TryGetInt(out var intValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, intValue);
                            }
                            break;
                        case "Int64":
                            isSuccess = value.TryGetLong(out var longValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, longValue);
                            }
                            break;
                        case "Double":
                            isSuccess = value.TryGetDouble(out var doubleValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, doubleValue);
                            }
                            break;
                        case "Single":
                            isSuccess = value.TryGetFloat(out var floatValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, floatValue);
                            }
                            break;
                        case "String":
                            isSuccess = value.TryGetString(out var stringValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, stringValue);
                            }
                            break;
                        default:
                            propertyInfo.SetValue(destObj, value);
                            break;
                    }
                }
            }
        }
    }
}
