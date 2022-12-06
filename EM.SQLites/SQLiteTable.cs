using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    _propertyAndTableInfos = GetPropertyAndTableInfos<T>();
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
        /// <param name="reader">数据行的可读流</param>
        /// <param name="successFields">成功的字段</param>
        /// <param name="propertyAndTableInfos">属性与表字段信息字典</param>
        /// <returns>对象</returns>
        protected T1 GetObject<T1>(DbDataReader reader, out List<string> successFields, Dictionary<PropertyInfo, TableInfo> propertyAndTableInfos) where T1 : new()
        {
            T1 t = default;
            successFields = new List<string>();
            if (reader == null)
            {
                return t;
            }
            t = new T1();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var value = reader.GetValue(i);
                if (propertyAndTableInfos != null)
                {
                    foreach (var item in propertyAndTableInfos)
                    {
                        if (item.Value.Name == fieldName)
                        {
                            if (DbToProperty(value, t, item.Key))
                            {
                                successFields.Add(fieldName);
                                continue;
                            }
                            break;
                        }
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
            List<T> ret = await GetObjectsAsync<T>(sql, parameters, PropertyAndTableInfos);
            return ret;
        }
        /// <summary>
        /// 从数据库读取对象集合
        /// </summary>
        /// <typeparam name="T1">类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="propertyAndTableInfos">属性和表信息映射集合</param>
        /// <returns>对象集合</returns>
        protected async Task<List<T1>> GetObjectsAsync<T1>(string sql, IEnumerable<DbParameter> parameters = null, Dictionary<PropertyInfo, TableInfo> propertyAndTableInfos = null) where T1 : new()
        {
            List<T1> ret = new List<T1>();
            if (string.IsNullOrEmpty(sql) || Connection == null)
            {
                return ret;
            }
            Dictionary<PropertyInfo, TableInfo> destPropertyAndTableInfos = propertyAndTableInfos ?? GetPropertyAndTableInfos<T1>();
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
                            var t = GetObject<T1>(reader, out _, destPropertyAndTableInfos);
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
        /// 获取对象集合
        /// </summary>
        /// <returns>对象集合</returns>
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
        /// 插入对象
        /// </summary>
        /// <param name="fieldAndValuesList">字段和值集合</param>
        /// <returns>成功个数</returns>
        public virtual async Task<int> InsertAsync(List<Dictionary<TableInfo, object>> fieldAndValuesList)
        {
            int ret = 0;
            if (Connection == null || string.IsNullOrEmpty(Name) || !(fieldAndValuesList?.Count() > 0))
            {
                return ret;
            }
            await Connection.ExecuteAsync(async () =>
            {
                int count = 0;
                foreach (var item in fieldAndValuesList)
                {
                    var sqlAndParas = GetInsertSql(Name, item);
                    count = await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
                    ret += count;
                }
            }, true);
            return ret;
        }
        /// <summary>
        /// 插入对象
        /// </summary>
        /// <param name="fieldAndValues">字段和值集合</param>
        /// <returns>成功与否</returns>
        public virtual async Task<bool> InsertAsync(Dictionary<TableInfo, object> fieldAndValues)
        {
            bool ret = false;
            if (Connection == null || string.IsNullOrEmpty(Name) || !(fieldAndValues?.Count() > 0))
            {
                return ret;
            }
            await Connection.ExecuteAsync(async () =>
            {
                var sqlAndParas = GetInsertSql(Name, fieldAndValues);
                var count = await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
            }, true);
            return ret;
        }
        /// <summary>
        /// 获取指定类型的属性与表信息映射集合
        /// </summary>
        /// <typeparam name="T1">指定类型</typeparam>
        /// <returns>属性与表信息映射集合</returns>
        public static Dictionary<PropertyInfo, TableInfo> GetPropertyAndTableInfos<T1>()
        {
            var ret = new Dictionary<PropertyInfo, TableInfo>();
            var type = typeof(T1);
            var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in propertyInfos)
            {
                if (propertyInfo.GetCustomAttribute(typeof(FieldAttribute)) is FieldAttribute fieldAttribute)
                {
                    ret.Add(propertyInfo, fieldAttribute.TableInfo);
                }
            }
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
            if (string.IsNullOrEmpty(tableName) || !(dic?.Count > 0))
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
            string ret;
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
                if (columnInfo.PrimaryKey > 0)//跳过主键
                {
                    continue;
                }
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
        public async Task UpdateAsync(T t)
        {
            if (Connection == null || string.IsNullOrEmpty(Name) || !(t is IdRecord idRecord))
            {
                return;
            }
            Dictionary<TableInfo, object> dic = GetTableInfoAndValues(t);
            var sqlAndParas = GetUpdateSql(Name, dic, GetIdFilter(idRecord.ID));
            await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
        }
        /// <summary>
        /// 更新多个对象
        /// </summary>
        /// <param name="ts">对象集合</param>
        /// <returns>成功个数</returns>
        public async Task UpdateAsync(IEnumerable<T> ts)
        {
            if (Connection == null || string.IsNullOrEmpty(Name) || !(ts?.Count() > 0))
            {
                return;
            }
            await Connection.ExecuteAsync(async () =>
            {
                foreach (var t in ts)
                {
                    if (t is IdRecord idRecord)
                    {
                        Dictionary<TableInfo, object> dic = GetTableInfoAndValues(t);
                        var sqlAndParas = GetUpdateSql(Name, dic, GetIdFilter(idRecord.ID));
                        await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
                    }
                }
            }, true);
        }
        /// <summary>
        /// 根据对象更新行
        /// </summary>
        /// <param name="fieldAndValues">对象</param>
        /// <param name="id">id</param>
        /// <returns>任务</returns>
        public async Task<bool> UpdateAsync(Dictionary<TableInfo, object> fieldAndValues, string id)
        {
            bool ret = false;
            if (Connection == null || string.IsNullOrEmpty(Name) || fieldAndValues == null || fieldAndValues.Count == 0 || string.IsNullOrEmpty(id))
            {
                return ret;
            }
            var sqlAndParas = GetUpdateSql(Name, fieldAndValues, GetIdFilter(id));
            var count = await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
            ret = count == 1;
            return ret;
        }
        /// <summary>
        /// 更新多个对象
        /// </summary>
        /// <param name="fieldAndValuesList">对象集合</param>
        /// <returns>成功个数</returns>
        public async Task<int> UpdateAsync(IEnumerable<(string Id, Dictionary<TableInfo, object> FieldAndValues)> fieldAndValuesList)
        {
            int ret = 0;
            if (Connection == null || string.IsNullOrEmpty(Name) || !(fieldAndValuesList?.Count() > 0))
            {
                return ret;
            }
            await Connection.ExecuteAsync(async () =>
            {
                foreach (var item in fieldAndValuesList)
                {
                    var sqlAndParas = GetUpdateSql(Name, item.FieldAndValues, GetIdFilter(item.Id));
                    var count = await Connection.ExecuteNonQueryAsync(sqlAndParas.Sql, sqlAndParas.Parameters);
                    ret += count;
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
        /// <summary>
        /// 获取表信息集合
        /// </summary>
        /// <returns>表信息集合</returns>
        public async Task<List<TableInfo>> GetTableInfosAsync()
        {
            if (Connection == null || string.IsNullOrEmpty(Name))
            {
                return new List<TableInfo>();
            }
            var sql = SQLiteQueries.GetTableInfoSql(Name);
            var ret = await GetObjectsAsync<TableInfo>(sql);
            return ret;
        }
        /// <summary>
        /// 获取注记集合
        /// </summary>
        /// <param name="fields">字段集合</param>
        /// <param name="filter">过滤条件</param>
        /// <returns>注记集合</returns>
        public async Task<List<Dictionary<string, object>>> GetFieldAndValuesListAsync(IEnumerable<string> fields = null, string filter = null)
        {
            if (Connection == null || string.IsNullOrEmpty(Name))
            {
                return new List<Dictionary<string, object>>();
            }
            var ret = await Connection.GetFieldAndValuesListAsync(Name, fields, filter);
            return ret;
        }
        /// <summary>
        /// 读取字段和值集合
        /// </summary>
        /// <param name="reader">数据读取器</param>
        /// <param name="fieldInfos">字段集合</param>
        /// <returns>字段和值集合</returns>
        protected Dictionary<TableInfo, object> GetFieldAndValues(DbDataReader reader, IEnumerable<TableInfo> fieldInfos)
        {
            var ret = new Dictionary<TableInfo, object>();
            if (reader == null || fieldInfos == null || fieldInfos.Count() == 0)
            {
                return ret;
            }
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var fieldInfo = fieldInfos.FirstOrDefault(x => x.Name == fieldName);
                if (fieldInfo != null && !ret.ContainsKey(fieldInfo))
                {
                    var dbValue = reader.GetValue(i);
                    var destValue = GetObjectFromDb(fieldInfo, dbValue);
                    ret.Add(fieldInfo, destValue);
                }
            }
            return ret;
        }
        /// <summary>
        /// 从数据库读取指定字段的值
        /// </summary>
        /// <param name="fieldInfo">字段信息</param>
        /// <param name="dbValue">数据库中的值</param>
        /// <returns>值</returns>
        protected virtual object GetObjectFromDb(TableInfo fieldInfo, object dbValue)
        {
            object ret = null;
            if (!DBNull.Value.Equals(dbValue))
            {
                ret = dbValue;
            }
            return ret;
        }
        /// <summary>
        /// 获取注记集合
        /// </summary>
        /// <param name="fieldInfos">字段集合</param>
        /// <param name="filter">过滤条件</param>
        /// <returns>注记集合</returns>
        public async Task<List<Dictionary<TableInfo, object>>> GetFieldAndValuesListAsync(IEnumerable<TableInfo> fieldInfos, string filter = null)
        {
            var ret = new List<Dictionary<TableInfo, object>>();
            if (Connection == null || string.IsNullOrEmpty(Name))
            {
                return ret;
            }
            var fields = fieldInfos?.Select(x => x.Name);
            var sql = GetSelectSql(Name, fieldInfos, filter: filter);
            if (Connection == null || string.IsNullOrEmpty(sql))
            {
                return ret;
            }
            await Connection.ExecuteAsync(async () =>
            {
                using (var cmd = Connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var fieldAndValues = GetFieldAndValues(reader, fieldInfos);
                            if (fieldAndValues != null)
                            {
                                ret.Add(fieldAndValues);
                            }
                        }
                        reader.Close();
                    }
                }
            });
            return ret;
        }
    }
}
