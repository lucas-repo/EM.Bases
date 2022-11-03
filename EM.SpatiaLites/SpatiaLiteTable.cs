using EM.SQLites;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EM.SpatiaLites
{
    /// <summary>
    /// 空间表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SpatiaLiteTable<T> : SQLiteTable<T> where T : IdRecord, new()
    {
        /// <summary>
        /// 几何列，每个空间表对应一条几何列记录
        /// </summary>
        public GeometryColumnRecord GeometryColumn { get; }
        public SpatiaLiteTable(DbConnection connection, string name, GeometryColumnRecord geometryColumn) : base(connection, name)
        {
            GeometryColumn = geometryColumn;
        }
        protected override (string SqlValue, DbParameter Parameter) ColumnAndValueToSql(TableInfo columnInfo, object value)
        {
            string sqlValue = null;
            DbParameter parameter = null;
            if (columnInfo == null)
            {
                return (null, null);
            }
            switch (columnInfo.Type)
            {
                case FieldType.INTEGER:
                case FieldType.REAL:
                    sqlValue = $"{value}";
                    break;
                case FieldType.BLOB:
                    if (value is byte[])
                    {
                        sqlValue = $"@{columnInfo.Name}";
                        parameter = new SQLiteParameter(columnInfo.Name, value);
                    }
                    else
                    {
                        Debug.WriteLine($"{columnInfo.Name}的值必须为字节数组");
                        return (null, null);
                    }
                    break;
                case GeoFieldType.GEOMETRY:
                    if (value is string wkt)
                    {
                        sqlValue = $" GeomFromText('{wkt}', {GeometryColumn.Srid})";
                    }
                    else
                    {
                        Debug.WriteLine($"{columnInfo.Name} 必须为GeometryObject");
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

        protected override string ColumnInfoToFieldSql(TableInfo columnInfo)
        {
            string ret = null;
            if (columnInfo != null)
            {
                switch (columnInfo.Type)
                {
                    case GeoFieldType.GEOMETRY:
                        ret = $"ST_AsText(\"{columnInfo.Name}\") AS {columnInfo.Name}";//转成wkt
                        break;
                    default:
                        ret = $"\"{columnInfo.Name}\"";
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 根据范围查找
        /// </summary>
        /// <param name="xmin">最小x</param>
        /// <param name="ymin">最小y</param>
        /// <param name="xmax">最大x</param>
        /// <param name="ymax">最大y</param>
        /// <returns>对象集合</returns>
        public async Task<List<T>> GetObjectsAsync(double xmin, double ymin, double xmax, double ymax)
        {
            if (Connection == null || string.IsNullOrEmpty(Name))
            {
                return new List<T>();
            }
            var sql = SpatiaLiteQueries.GetSelectByExtentSql(Name, xmin, ymin, xmax, ymax);
            var ret = await GetObjectsAsync(sql);
            return ret;
        }
    }
}
