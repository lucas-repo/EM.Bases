using EM.SQLites;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM.SpatiaLites
{
    /// <summary>
    ///  SpatiaLite常用查询语句
    /// </summary>
    public static class SpatiaLiteQueries
    {
        /// <summary>
        /// 几何列的表名
        /// </summary>
        public const string GeometryColumnTableName = "geometry_columns";
        /// <summary>
        /// 初始化空间元数据。在创建新数据库之后，在尝试调用任何其他 Spatial SQL 函数之前，必须立即调用 InitSspaceMetaData() 函数
        /// </summary>
        public const string InitSpatialMetaDataSql = "SELECT InitSpatialMetaData()";
        /// <summary>
        /// 获取创建空间表的sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="tableInfos">表信息集合</param>
        /// <returns>sql</returns>
        public static string GetCreateSpatialTableSql(string tableName, IEnumerable<TableInfo> tableInfos)
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
                    if ( item.Type == GeoFieldType.GEOMETRY)
                    {
                        continue;//创建普通表时，不允许添加几何列，应使用GetAddGeometryColumnSql()方法
                    }
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
        /// 获取添加几何列的sql
        /// </summary>
        /// <param name="geometryColumn">几何列</param>
        /// <returns>sql</returns>
        public static string GetAddGeometryColumnSql(this GeometryColumnRecord geometryColumn)
        {
            string ret = null;
            if (geometryColumn != null)
            {
                var geometryType = GeometryColumnRecord.GetGeometryType(geometryColumn.GeometryType);
                var coordDimension = CoordDimension.GetCoordDimensionString(geometryColumn.CoordDimension, geometryColumn.GeometryType);
                ret = $"SELECT AddGeometryColumn('{geometryColumn.TableName}', '{geometryColumn.GeometryColumn}',{geometryColumn.Srid}, '{geometryType}', '{coordDimension}',{geometryColumn.SpatialIndexEnabled});";
            }
            return ret;
        }
        /// <summary>
        /// 获取创建空间索引sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="geometryColumn">几何列名</param>
        /// <returns>创建空间索引sql</returns>
        public static string GetCreateSpatialIndexSql(string tableName, string geometryColumn)
        {
            string ret = null;
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(geometryColumn))
            {
                return ret;
            }
            ret = $"SELECT CreateSpatialIndex('{tableName}', '{geometryColumn}');";
            return ret;
        }
        // <summary>
        /// 获取删除空间索引sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="geometryColumn">几何列名</param>
        /// <returns>删除空间索引sql</returns>
        public static string GetDisableSpatialIndexSql(string tableName, string geometryColumn)
        {
            string ret = null;
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(geometryColumn))
            {
                return ret;
            }
            ret = $"SELECT DisableSpatialIndex('{tableName}', '{geometryColumn}');";
            return ret;
        }
        /// <summary>
        /// 获取删除几何列触发器的sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="geometryColumn">几何列名</param>
        /// <returns>sql</returns>
        public static string GetDiscardGeometryColumnSql(string tableName, string geometryColumn)
        {
            string ret = null;
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(geometryColumn))
            {
                return ret;
            }
            ret = $"SELECT DiscardGeometryColumn('{tableName}', '{geometryColumn}');";
            return ret;
        }
        /// <summary>
        /// 获取恢复几何列的sql，这将尝试重新创建与给定几何体相关的任何元数据和任何触发器，旨在在第二次恢复已经存在（并填充）列
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="geometryColumn">几何列</param>
        /// <returns>sql</returns>
        public static string GetRecoverGeometryColumnSql(GeometryColumnRecord geometryColumn)
        {
            string ret = null;
            if ( geometryColumn == null)
            {
                return ret;
            }
            var geometryType = GeometryColumnRecord.GetGeometryType(geometryColumn.GeometryType);
            var coordDimension = CoordDimension.GetCoordDimensionString(geometryColumn.CoordDimension, geometryColumn.GeometryType);
            ret = $"SELECT RecoverGeometryColumn('{geometryColumn.TableName}', '{geometryColumn.GeometryColumn}',{geometryColumn.Srid},‘{geometryType}’,'{coordDimension}');";
            return ret;
        }
        /// <summary>
        /// 获取根据范围查询的sql
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="xmin">最小x</param>
        /// <param name="ymin">最小y</param>
        /// <param name="xmax">最大x</param>
        /// <param name="ymax">最大y</param>
        /// <returns>sql</returns>
        public static string GetSelectByExtentSql(string tableName,double xmin,double ymin,double xmax,double ymax)
        {
            string ret = null;
            if (string.IsNullOrEmpty(tableName))
            {
                return ret;
            }
            ret = $"SELECT * FROM {tableName} WHERE ROWID IN ( SELECT pkid FROM idx_{tableName}_geometry WHERE NOT ( {xmax} < xmin OR {xmin} > xmax OR {ymin} > ymax OR {ymax} < ymin ) );";
            return ret;
        }
    }
}
