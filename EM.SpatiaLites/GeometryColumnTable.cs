using EM.SQLites;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace EM.SpatiaLites
{
    /// <summary>
    /// 几何列表
    /// </summary>
    public class GeometryColumnTable : SQLiteTable<GeometryColumnRecord>
    {
        /// <summary>
        /// 几何列
        /// </summary>
        public const string GeometryColumn = "geometry";
        public GeometryColumnTable(DbConnection connection, string name) : base(connection, name)
        {
        }

        /// <summary>
        /// 查询几何列记录
        /// </summary>
        /// <param name="annotationTableName">表名</param>
        /// <param name="geometryColumn">几何列</param>
        /// <returns>几何列记录</returns>
        public GeometryColumnRecord GetObject(string annotationTableName, string geometryColumn = GeometryColumn)
        {
            GeometryColumnRecord ret = null;
            if (string.IsNullOrEmpty(annotationTableName) || string.IsNullOrEmpty(geometryColumn))
            {
                return ret;
            }
            string filter = $"f_table_name='{annotationTableName}' AND f_geometry_column='{geometryColumn}'";
            string sql = GetSelectSql(Name, PropertyAndTableInfos.Values, index: -1, filter: filter);
            var geometryColumns =  GetObjects(sql);
            ret = geometryColumns.FirstOrDefault();
            return ret;
        }
        public override bool Insert(GeometryColumnRecord t)
        {
            throw new Exception($"请使用{nameof(SpatiaLiteContext.CreateSpatialTable)}方法添加空间表");
        }
        public override void Delete(string id)
        {
            throw new Exception($"不允许直接删除记录");
        }
        public override int Delete(IEnumerable<string> ids, bool useTransaction = true)
        {
            throw new Exception($"不允许直接删除记录");
        }
    }
}
