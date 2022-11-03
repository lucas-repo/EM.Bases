using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EM.SQLites
{
    /// <summary>
    /// 表
    /// </summary>
    /// <typeparam name="T">类或字典。为类时，将带<seealso cref="FieldAttribute"/>特性的公开属性写到数据库/></typeparam>
    public class SQLiteTable<T> where T : Record, new()
    {
        /// <summary>
        /// 数据库
        /// </summary>
        public DbConnection Connection { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }

        private Dictionary<PropertyInfo, TableInfo> _propertyAndTableInfos;
        /// <summary>
        /// 类型<seealso cref="T"/>中带<seealso cref="FieldAttribute"/>特性的属性信息和字段信息集合
        /// </summary>
        protected Dictionary<PropertyInfo, TableInfo> PropertyAndTableInfos
        {
            get
            {
                if (_propertyAndTableInfos == null)
                {
                    _propertyAndTableInfos = new Dictionary<PropertyInfo, TableInfo>();
                    var type = typeof(T);
                    var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var propertyInfo in propertyInfos)
                    {
                        if (propertyInfo.GetCustomAttribute(typeof(FieldAttribute)) is FieldAttribute fieldAttribute)
                        {
                            _propertyAndTableInfos.Add(propertyInfo, fieldAttribute.TableInfo);
                        }
                    }
                }
                return _propertyAndTableInfos;
            }
        }

        /// <summary>
        /// 实例化<seealso cref="SQLiteTable{T}"/>
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="name">表名</param>
        /// <exception cref="ArgumentNullException">空引用参数异常</exception>
        public SQLiteTable(DbConnection connection, string name)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// 获取个数
        /// </summary>
        public async Task<int> GetCountAsync()
        {
            int ret = 0;
            if (Connection == null || string.IsNullOrEmpty(Name))
            {
                return ret;
            }
            string sql = SQLiteQueries.GetCountSql(Name);
            if (await Connection.ExecuteScalarAsync(sql) is int count)
            {
                ret = count;
            }
            return ret;
        }
        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>对象</returns>
        public async Task<T> GetObjectAsync(int index)
        {
            if (Connection == null || string.IsNullOrEmpty(Name))
            {
                return default;
            }
            List<T> values = await GetObjectsAsync(index, 1);
            T t = values.FirstOrDefault();
            return t;
        }
        /// <summary>
        /// 将数据库的字段值写入指定对象的属性（名称需一致）
        /// </summary>
        /// <param name="fieldValue">字段值</param>
        /// <param name="destObj">指定对象</param>
        /// <param name="propertyInfo">属性信息</param>
        /// <returns>成功true，反之false</returns>
        protected virtual bool DbToProperty(object fieldValue, object destObj, PropertyInfo propertyInfo)
        {
            bool ret = false;
            if (DBNull.Value.Equals(fieldValue) || fieldValue == null || destObj == null || propertyInfo == null)
            {
                return ret;
            }
            try
            {
                propertyInfo.ForceSetValue(destObj, fieldValue);
                ret = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(DbToProperty)} 写入属性 {propertyInfo.Name} 失败，{e}");
            }
            return ret;
        }
        /// <summary>
        /// 从数据库中读取对象
        /// </summary>
        /// <typeparam name="T">对象</typeparam>
        /// <param name="reader">数据行的可读流</param>
        /// <param name="successFields">成功的字段</param>
        /// <returns>对象</returns>
        protected T GetObject(DbDataReader reader, out List<string> successFields)
        {
            T t = default;
            successFields = new List<string>();
            if (reader == null)
            {
                return t;
            }
            t = new T();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                foreach (var item in PropertyAndTableInfos)
                {
                    if (item.Value.Name == fieldName)
                    {
                        var value = reader.GetValue(i);
                        if (DbToProperty(value, t, item.Key))
                        {
                            successFields.Add(fieldName);
                        }
                        break;
                    }
                }
            }
            return t;
        }
        /// <summary>
        /// 根据sql和参数查询数据库，并返回对象集合
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>对象集合</returns>
        public async Task<List<T>> GetObjectsAsync(string sql, IEnumerable<DbParameter> parameters = null)
        {
            List<T> ret = new List<T>();
            if (string.IsNullOrEmpty(sql) || Connection == null)
            {
                return ret;
            }
            await Connection.ExecuteAsync(async () =>
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    if (parameters?.Count() > 0)
                    {
                        foreach (var item in parameters)
                        {
                            cmd.Parameters.Add(item);
                        }
                    }
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            T t = GetObject(reader, out _);
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
        /// 获取查询sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableInfos">表信息</param>
        /// <param name="index">索引，为负数时查询所有行</param>
        /// <param name="count">个数</param>
        /// <param name="filter">条件</param>
        /// <returns>sql</returns>
        public string GetSelectSql(string tableName, IEnumerable<TableInfo> tableInfos, int index = -1, int count = 1, string filter = null)
        {
            string ret = null;
            if (string.IsNullOrEmpty(tableName))
            {
                return ret;
            }
            StringBuilder sb = new StringBuilder();
            if (tableInfos?.Count() > 0)
            {
                for (int i = 0; i < tableInfos.Count(); i++)
                {
                    var columnInfo = tableInfos.ElementAt(i);
                    string colSql = ColumnInfoToFieldSql(columnInfo);
                    if (string.IsNullOrEmpty(colSql))
                    {
                        continue;
                    }
                    sb.Append(i == 0 ? colSql : $",{colSql}");
                }
            }
            else
            {
                sb.Append("*");
            }
            string fields = sb.ToString();

            string limit = index < 0 ? string.Empty : $" LIMIT {index},{count}";
            string where = string.IsNullOrEmpty(filter) ? string.Empty : $" WHERE {filter}";
            ret = $"select {fields} FROM {tableName}{limit}{where}";
            return ret;
        }
        /// <summary>
        /// 将列信息转为sql中的查询列
        /// </summary>
        /// <param name="tableInfo">列信息</param>
        /// <returns>查询列</returns>
        protected virtual string ColumnInfoToFieldSql(TableInfo tableInfo)
        {
            string ret = string.Empty;
            if (!string.IsNullOrEmpty(tableInfo?.Name))
            {
                ret = $"\"{tableInfo.Name}\"";
            }
            return ret;
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>对象</returns>
        public async Task<List<T>> GetObjectsAsync()
        {
            if (Connection == null || string.IsNullOrEmpty(Name))
            {
                return new List<T>();
            }
            string sql = GetSelectSql(Name, PropertyAndTableInfos.Values, index: -1);
            List<T> ret = await GetObjectsAsync(sql);
            return ret;
        }

        /// <summary>
        /// 获取对象集合
        /// </summary>
        /// <param name="index">索引，为负数时查询所有行</param>
        /// <param name="count">个数</param>
        /// <param name="filter">过滤条件</param>
        /// <returns>对象集合</returns>
        public async Task<List<T>> GetObjectsAsync(int index = -1, int count = 1, string filter = null)
        {
            if (Connection == null || string.IsNullOrEmpty(Name))
            {
                return new List<T>();
            }
            string sql = GetSelectSql(Name, PropertyAndTableInfos.Values, index, count, filter);
            List<T> ret = await GetObjectsAsync(sql);
            return ret;
        }

        /// <summary>
        /// 获取插入数据查询语句和参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableInfoAndValues">列值集合</param>
        /// <returns>查询语句和参数</returns>
        protected (string Sql, List<DbParameter> Parameters) GetInsertSql(string tableName, Dictionary<TableInfo, object> tableInfoAndValues)
        {
            (string Sql, List<DbParameter> Parameters) ret = (null, new List<DbParameter>());
            if (string.IsNullOrEmpty(tableName) || !(tableInfoAndValues?.Count > 0))
            {
                return ret;
            }
            Dictionary<TableInfo, object> destDic = new Dictionary<TableInfo, object>();
            foreach (var item in tableInfoAndValues)
            {
                //如果有自增列可自行排除
                destDic.Add(item.Key, item.Value);
            }
            if (destDic.Count == 0)
            {
                return ret;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append($"INSERT INTO {tableName}(");
            for (int i = 0; i < destDic.Count; i++)
            {
                var columnInfo = destDic.ElementAt(i).Key;
                sb.Append(i == 0 ? columnInfo.Name : $",{columnInfo.Name}");
            }
            sb.Append(")VALUES(");
            for (int i = 0; i < destDic.Count; i++)
            {
                var item = destDic.ElementAt(i);
                var columnInfo = item.Key;
                var value = item.Value;
                var sqlAndPara = ColumnAndValueToSql(columnInfo, value);
                sb.Append(i == 0 ? $"{sqlAndPara.SqlValue}" : $",{sqlAndPara.SqlValue}");
                if (sqlAndPara.Parameter != null)
                {
                    ret.Parameters.Add(sqlAndPara.Parameter);
                }
            }
            sb.Append(");");
            ret.Sql = sb.ToString();
            return ret;
        }
        /// <summary>
        /// 将列的值插入sql查询语句
        /// </summary>
        /// <param name="tableInfo">列信息</param>
        /// <param name="value">值</param>
        /// <returns>sql部分语句和参数</returns>
        protected virtual (string SqlValue, DbParameter Parameter) ColumnAndValueToSql(TableInfo tableInfo, object value)
        {
            string sqlValue = null;
            DbParameter parameter = null;
            if (tableInfo == null)
            {
                return (null, null);
            }
            switch (tableInfo.Type)
            {
                case FieldType.INTEGER:
                case FieldType.REAL:
                    sqlValue = $"{value}";
                    break;
                case FieldType.BLOB:
                    if (value is byte[])
                    {
                        sqlValue = $"@{tableInfo.Name}";
                        parameter = new SQLiteParameter(tableInfo.Name, value);
                    }
                    else
                    {
                        Debug.WriteLine($"{tableInfo.Name}的值必须为字节数组");
                        return (null, null);
                    }
                    break;
                case FieldType.TEXT:
                default://默认的按文本处理
                    sqlValue = $"'{value}'";
                    break;
            }
            return (sqlValue, parameter);
        }
        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="t">对象</param>
        /// <returns>成功true反之false</returns>
        public virtual async Task<bool> InsertAsync(T t)
        {
            bool ret = false;
            if (Connection == null || string.IsNullOrEmpty(Name) || t == null)
            {
                return ret;
            }
            Dictionary<TableInfo, object> tableInfoAndValues = GetTableInfoAndValues(t);
            var sqlAndParas = GetInsertSql(Name, tableInfoAndValues);
            var count = await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
            ret = count == 1;
            return ret;
        }
        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="ts">对象集合</param>
        /// <returns>成功个数</returns>
        public virtual async Task<int> InsertAsync(IEnumerable<T> ts)
        {
            int ret = 0;
            if (Connection == null || string.IsNullOrEmpty(Name) || !(ts?.Count() > 0))
            {
                return ret;
            }
            await Connection.ExecuteAsync(async () =>
            {
                int count = 0;
                foreach (var item in ts)
                {
                    Dictionary<TableInfo, object> tableInfoAndValues = GetTableInfoAndValues(item);
                    var sqlAndParas = GetInsertSql(Name, tableInfoAndValues);
                    count = await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
                    ret += count;
                }
            }, true);
            return ret;
        }

        /// <summary>
        /// 将对象转为列和值字典
        /// </summary>
        /// <param name="t">对象</param>
        /// <returns>列和值字典</returns>
        public Dictionary<TableInfo, object> GetTableInfoAndValues(T t)
        {
            Dictionary<TableInfo, object> ret = new Dictionary<TableInfo, object>();
            if (t != null)
            {
                if (t is Dictionary<TableInfo, object>)
                {
                    ret = t as Dictionary<TableInfo, object>;
                }
                else
                {
                    var type = typeof(T);
                    var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var propertyInfo in propertyInfos)
                    {
                        if (propertyInfo.GetCustomAttribute(typeof(FieldAttribute)) is FieldAttribute columnAttribute)
                        {
                            var value = propertyInfo.GetValue(t);
                            ret.Add(columnAttribute.TableInfo, value);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 获取更新空间数据查询语句和参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">列值集合</param>
        /// <param name="id">id</param>
        /// <returns>查询语句和参数</returns>
        protected (string Sql, List<DbParameter> Parameters) GetUpdateSqlById(string tableName, Dictionary<TableInfo, object> dic, string id)
        {
            if (string.IsNullOrEmpty(tableName) || !(dic?.Count > 0) )
            {
                return (null, new List<DbParameter>());
            }
            var ret = GetUpdateSql(tableName, dic, GetIdFilter(id));
            return ret;
        }
        /// <summary>
        /// 获取id过滤条件
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>过滤条件</returns>
        private string GetIdFilter(string id)
        {
            string ret = null;
            Type type = typeof(IdRecord);
            if (type.IsAssignableFrom(typeof(T)))
            {
                ret = $"id='{id}'";
            }
            else
            {
                throw new NotImplementedException();
            }
            return ret;
        }
        /// <summary>
        /// 获取更新空间数据查询语句和参数
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dic">列值集合</param>
        /// <param name="filter">过滤条件</param>
        /// <returns>查询语句和参数</returns>
        protected (string Sql, List<DbParameter> Parameters) GetUpdateSql(string tableName, Dictionary<TableInfo, object> dic, string filter = null)
        {
            (string Sql, List<DbParameter> Parameters) ret = (null, new List<DbParameter>());
            if (string.IsNullOrEmpty(tableName) || !(dic?.Count > 0))
            {
                return ret;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append($"UPDATE {tableName} SET ");
            for (int i = 0; i < dic.Count; i++)
            {
                var item = dic.ElementAt(i);
                var columnInfo = item.Key;
                var value = item.Value;
                sb.Append(i == 0 ? $"{columnInfo.Name} = " : $",{columnInfo.Name} = ");
                var sqlAndPara = ColumnAndValueToSql(columnInfo, value);
                sb.Append(sqlAndPara.SqlValue);
                if (sqlAndPara.Parameter != null)
                {
                    ret.Parameters.Add(sqlAndPara.Parameter);
                }
            }
            if (!string.IsNullOrEmpty(filter))
            {
                sb.Append($" WHERE {filter}");
            }
            sb.Append(";");
            ret.Sql = sb.ToString();
            return ret;
        }

        /// <summary>
        /// 根据对象更新行
        /// </summary>
        /// <param name="t">对象</param>
        /// <returns>任务</returns>
        public async Task<bool> UpdateAsync(T t)
        {
            bool ret = false;
            if (Connection == null || string.IsNullOrEmpty(Name) || !(t is IdRecord idRecord))
            {
                return ret;
            }
            Dictionary<TableInfo, object> dic = GetTableInfoAndValues(t);
            var sqlAndParas = GetUpdateSql(Name, dic, idRecord.ID);
            var count = await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
            ret = count == 1;
            return ret;
        }
        /// <summary>
        /// 更新多个对象
        /// </summary>
        /// <param name="ts">对象集合</param>
        /// <returns>成功个数</returns>
        public async Task<int> UpdateAsync(IEnumerable<T> ts)
        {
            int ret = 0;
            if (Connection == null || string.IsNullOrEmpty(Name) || !(ts?.Count() > 0))
            {
                return ret;
            }
            await Connection.ExecuteAsync(async () =>
            {
                foreach (var t in ts)
                {
                    if (t is IdRecord idRecord)
                    {
                        Dictionary<TableInfo, object> dic = GetTableInfoAndValues(t);
                        var sqlAndParas = GetUpdateSql(Name, dic, idRecord.ID);
                        var count = await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
                        ret += count;
                    }
                }
            }, true);
            return ret;
        }
        /// <summary>
        /// 删除指定行id的数据
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>任务</returns>
        public virtual async Task DeleteAsync(string id)
        {
            if (Connection == null || string.IsNullOrEmpty(Name))
            {
                return;
            }
            var sql = GetDeleteSql(Name, id);
            await Connection.ExecuteNonQueryAsync(sql);
        }

        /// <summary>
        /// 获取删除sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="id">id</param>
        /// <returns>sql</returns>
        public string GetDeleteSql(string tableName, string id)
        {
            string ret = string.Empty;
            if (string.IsNullOrEmpty(tableName))
            {
                return ret;
            }
            ret = SQLiteQueries.GetDeleteSql(tableName, GetIdFilter(id));
            return ret;
        }
        /// <summary>
        /// 删除多个指定行id的数据
        /// </summary>
        /// <param name="ids">行id集合（从1开始）</param>
        /// <returns>任务</returns>
        public virtual async Task<int> DeleteAsync(IEnumerable<string> ids)
        {
            int ret = 0;
            if (Connection == null || string.IsNullOrEmpty(Name) || !(ids?.Count() > 0))
            {
                return ret;
            }
            await Connection.ExecuteAsync(async () =>
            {
                foreach (var id in ids)
                {
                    var sql = GetDeleteSql(Name, id);
                    var count = await Connection.ExecuteNonQueryAsync(sql);
                    ret += count;
                }
            }, true);
            return ret;
        }
    }
}
