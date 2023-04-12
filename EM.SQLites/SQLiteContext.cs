using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace EM.SQLites
{
    /// <summary>
    /// SQLite数据库上下文
    /// </summary>
    public abstract class SQLiteContext : IDisposable
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        public SQLiteConnection Connection { get; private set; }
        /// <summary>
        /// 数据库连接状态
        /// </summary>
        public ConnectionState ConnectionState
        {
            get
            {
                ConnectionState state = ConnectionState.Closed;
                if (Connection != null)
                {
                    state = Connection.State;
                }
                return state;
            }
        }

        public SQLiteContext(SQLiteConnection connection)
        {
            Connection = connection;
        }
        public SQLiteContext(string filename)
        {
            Connection = CreateDatabaseIfNotExists(filename);
        }
        /// <summary>
        /// 如果不存在数据库则创建，否则直接打开，并返回连接
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>连接</returns>
        public SQLiteConnection CreateDatabaseIfNotExists(string filename)
        {
            SQLiteConnection ret = null;
            if (string.IsNullOrEmpty(filename))
            {
                return ret;
            }
            if (File.Exists(filename))
            {
                var connectionString = SQLiteQueries.GetConnectionString(filename);
                ret = new SQLiteConnection(connectionString);
            }
            else
            {
                try
                {
                    var directory = Path.GetDirectoryName(filename);
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                    ret = DoCreateDatabase(filename);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{nameof(CreateDatabaseIfNotExists)}失败,{e}");
                }
            }
            return ret;
        }
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>连接</returns>
        protected virtual SQLiteConnection DoCreateDatabase(string filename)
        {
            SQLiteConnection ret = null;
            SQLiteConnection.CreateFile(filename);
            if (File.Exists(filename))
            {
                var connectionString = SQLiteQueries.GetConnectionString(filename);
                ret = new SQLiteConnection(connectionString);
                if (ret != null)
                {
                    var type = GetType();
                    var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    StringBuilder sb = new StringBuilder();
                    var tableType = typeof(SQLiteTable);
                    foreach (var propertyInfo in propertyInfos)
                    {
                        if (tableType.IsAssignableFrom(propertyInfo.PropertyType))
                        {
                            var value = propertyInfo.GetValue(this);
                            if (value is SQLiteTable table)
                            {
                                string tableName = table.Name;
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    continue;
                                }
                                sb.Append(SQLiteQueries.GetCreateTableSql(tableName, table.PropertyAndTableInfos.Values));
                                var str = GetCreateIndexString(table);
                                if (str.Length > 0)
                                {
                                    sb.Append(str);
                                }
                            }
                        }
                    }
                    Connection.ExecuteNonQuery(sb.ToString());
                }
            }
            return ret;
        }
        private string GetCreateIndexString(SQLiteTable table)
        {
            var type = table.GetType();
            var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<FieldAttribute> fields = new List<FieldAttribute>();
            foreach (var item in propertyInfos)
            {
                var field = item.GetCustomAttribute<FieldAttribute>();
                if (field == null || string.IsNullOrEmpty(field.IndexName))
                {
                    continue;
                }
                fields.Add(field);
            }
            var groupFields = fields.GroupBy(x => x.IndexName);
            StringBuilder sb = new StringBuilder();
            foreach (var item in groupFields)
            {
                var firstField = item.First();
                string indexStr = firstField.IsUnique ? $"UNIQUE INDEX" : "INDEX";
                sb.Append($"CREATE {indexStr} {firstField.IndexName} on {table.Name} (");
                int i = 0;
                int count = item.Count();
                foreach (var field in item)
                {
                    sb.Append(i == 0 ? field.TableInfo.Name : $",{field.TableInfo.Name}");
                    i++;
                }
                sb.Append(");");
            }
            return sb.ToString();
        }
        /// <summary>
        /// 创数据库
        /// </summary>
        /// <param name="filename">文件名</param>
        public static void CreateDatabase(string filename)
        {
            try
            {
                var directory = Path.GetDirectoryName(filename);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                SQLiteConnection.CreateFile(filename);//第一步，创建数据库
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(CreateDatabase)}_{filename}失败，{e}");
            }
        }

        /// <summary>
        /// 执行sql方法（自动打开数据库链接）
        /// </summary>
        /// <param name="action">要执行的sql查询方法</param>
        /// <param name="useTransaction">使用事务</param>
        /// <returns>任务</returns>
        public void Execute(Action action, bool useTransaction = false)
        {
            if (Connection == null || action == null)
            {
                return;
            }
            Connection.Execute(action, useTransaction);
        }
        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <returns></returns>
        public virtual void OpenConnection()
        {
            if (Connection != null && Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }
        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public virtual void CloseConnection()
        {
            if (Connection != null && Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (Connection != null)
            {
                CloseConnection();
                Connection.Dispose();
                Connection = null;
            }
        }

    }
}
