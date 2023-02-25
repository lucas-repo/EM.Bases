using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EM.SQLites
{
    /// <summary>
    /// SQLite常用查询语句
    /// </summary>
    public class SQLiteQueries
    {
        /// <summary>
        /// 元数据信息
        /// </summary>
        public const string MasterInfo = "SELECT * FROM sqlite_master;";//sqlite_master是SQLite用来存储内部对象的主元数据表
        /// <summary>
        /// 清理未使用的存储
        /// </summary>
        public const string Vacuum = "VACUUM;";
        /// <summary>
        /// 禁用检查约束（禁用检查约束是不安全的，但在初步数据加载期间可能需要）
        /// </summary>
        public const string IgnoreCheckConstraintSql = "PRAGMA ignore_check_constraint;";
        /// <summary>
        /// 数据库连接字符串前部分
        /// </summary>
        public const string CON_STR_PREFIX = "Data Source=";
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <returns>连接字符串</returns>
        public static string GetConnectionString(string filename)
        {
            return $"{CON_STR_PREFIX}{filename};";
        }
        /// <summary>
        /// 从数据库连接字符串中获取数据库文件路劲
        /// </summary>
        /// <param name="conStr">数据库连接字串</param>
        /// <returns>文件全路径</returns>
        public static string GetFileNameFromConString(string conStr)
        {
            if (conStr is null)
            {
                throw new ArgumentNullException(nameof(conStr));
            }

            var fileName = conStr.Replace(CON_STR_PREFIX, string.Empty)?.Replace(";", string.Empty);
            return fileName;
        }
        /// <summary>
        /// 获取创建表的sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableInfos">表信息集合</param>
        /// <returns>sql</returns>
        public static string GetCreateTableSql(string tableName, IEnumerable<TableInfo> tableInfos)
        {
            string ret = string.Empty;
            if (string.IsNullOrEmpty(tableName) || !(tableInfos?.Count() > 0))
            {
                return ret;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append($"CREATE TABLE {tableName} (");
            foreach (var item in tableInfos)
            {
                if (item != null)
                {
                    sb.Append($" '{item.Name}' {item.Type}");
                    switch (item.Type)
                    {
                        case FieldType.TEXT:
                        case FieldType.BLOB:
                            if (item.Notnull != 0)
                            {
                                sb.Append(" NOT NULL");
                            }
                            break;
                    }
                    sb.Append(",");
                }
            }
            var columns = tableInfos.Where(x => x.PrimaryKey > 0).OrderBy(x => x.PrimaryKey);
            if (columns.Any())
            {
                sb.Append("PRIMARY KEY (");
                foreach (var item in columns)
                {
                    sb.Append($" '{item.Name}'");
                }
                sb.Append(")");
            }
            sb.Append(");");
            ret = sb.ToString();
            return ret;
        }

        /// <summary>
        /// 获取表信息sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>sql</returns>
        public static string GetTableInfoSql(string tableName)
        {
            return $"PRAGMA table_info({tableName});";
        }
        /// <summary>
        /// 获取删除sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="filter">过滤条件</param>
        /// <returns>sql</returns>
        public static string GetDeleteSql(string tableName, string filter = null)
        {
            string ret = null;
            if (string.IsNullOrEmpty(tableName))
            {
                return ret;
            }
            if (string.IsNullOrEmpty(filter))
            {
                ret = $"DELETE FROM {tableName};";
            }
            else
            {
                ret = $"DELETE FROM {tableName} WHERE {filter};";
            }
            return ret;
        }

        /// <summary>
        /// 获取个数sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>sql</returns>
        public static string GetCountSql(string tableName)
        {
            string ret = null;
            if (string.IsNullOrEmpty(tableName))
            {
                return ret;
            }
            ret = $"select COUNT(*) FROM {tableName};";
            return ret;
        }
        /// <summary>
        /// 获取查询语句
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="fields">字段集合</param>
        /// <param name="filter">过滤条件</param>
        /// <returns>查询语句</returns>
        public static string GetSelectSql(string tableName, IEnumerable<string> fields = null, string filter = null)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            if (fields == null || fields.Count() == 0)
            {
                sb.Append("*");
            }
            else
            {
                for (int i = 0; i < fields.Count(); i++)
                {
                    if (i == 0)
                    {
                        sb.Append(fields.ElementAt(i));
                    }
                    else
                    {
                        sb.Append($",{fields.ElementAt(i)}");
                    }
                }
            }
            sb.Append($" FROM {tableName} ");
            if (!string.IsNullOrEmpty(filter))
            {
                sb.Append($" WHERE {filter}");
            }
            sb.Append(";");
            return sb.ToString();
        }
    }

}
