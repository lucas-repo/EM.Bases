namespace EM.SpatiaLites
{
    /// <summary>
    /// 几何类型
    /// </summary>
    public enum GeometryType
    {
        /// <summary>
        /// 几何体
        /// </summary>
        GEOMETRY,
        /// <summary>
        /// 点
        /// </summary>
        POINT,
        /// <summary>
        /// 线
        /// </summary>
        LINESTRING,
        /// <summary>
        /// 面
        /// </summary>
        POLYGON,
        /// <summary>
        /// 多点
        /// </summary>
        MULTIPOINT,
        /// <summary>
        /// 多线
        /// </summary>
        MULTILINESTRING,
        /// <summary>
        /// 多面
        /// </summary>
        MULTIPOLYGON,
        /// <summary>
        /// 几何体集合
        /// </summary>
        GEOMETRYCOLLECTION,
    }
}
