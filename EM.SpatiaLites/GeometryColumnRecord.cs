using EM.SQLites;

namespace EM.SpatiaLites
{
    /// <summary>
    /// 几何列
    /// </summary>
    public class GeometryColumnRecord : Record
    {
        private string _tableName;
        /// <summary>
        /// 表名
        /// </summary>
        [Field("f_table_name", FieldType.TEXT)]
        public string TableName
        {
            get { return _tableName; }
            set { SetProperty(ref _tableName, value); }
        }
        private string _geometryColumn;
        /// <summary>
        /// 几何列
        /// </summary>
        [Field("f_geometry_column", FieldType.TEXT)]
        public string GeometryColumn
        {
            get { return _geometryColumn; }
            set { SetProperty(ref _geometryColumn, value); }
        }
        private int _geometryType;
        /// <summary>
        /// 几何类型
        /// </summary>
        [Field("geometry_type", FieldType.INTEGER)]
        public int GeometryType
        {
            get { return _geometryType; }
            set { SetProperty(ref _geometryType, value); }
        }
        private int _coordDimension;
        /// <summary>
        /// 坐标维度
        /// </summary>
        [Field("coord_dimension", FieldType.INTEGER)]
        public int CoordDimension
        {
            get { return _coordDimension; }
            set { SetProperty(ref _coordDimension, value); }
        }
        private int _srid;
        /// <summary>
        /// 空间参考
        /// </summary>
        [Field("srid", FieldType.INTEGER)]
        public int Srid
        {
            get { return _srid; }
            set { SetProperty(ref _srid, value); }
        }
        private int _spatialIndexEnabled;
        /// <summary>
        /// 为0时允许<seealso cref="f_geometry_column"/>为空，否则不允许为空，默认为0
        /// </summary>
        [Field("spatial_index_enabled", FieldType.INTEGER)]
        public int SpatialIndexEnabled
        {
            get { return _spatialIndexEnabled; }
            set { SetProperty(ref _spatialIndexEnabled, value); }
        }
        /// <summary>
        /// 根据<seealso cref="GeometryType"/>和<seealso cref="CoordDimension"/>，返回几何类型。
        /// 维度为XY时，GeometryType取值0-7。
        /// 维度为XYZ时，GeometryType取值1000-1007。
        /// 维度为XYM时，GeometryType取值2000-2007。
        /// 维度为XYZM时，GeometryType取值3000-3007。
        /// </summary>
        /// <param name="geometryType">几何类型值</param>
        /// <returns>几何类型</returns>
        public static GeometryType GetGeometryType(int geometryType)
        {
            int intValue = geometryType % 1000;
            GeometryType ret = (GeometryType)intValue;
            return ret;
        }
    }
}
