using EM.SQLites;
using System;

namespace EM.SpatiaLites
{
    /// <summary>
    /// 带几何字段的数据
    /// </summary>
    [Serializable]
    public class GeometryRecord:IdRecord
    {
        private string _geometry;
        /// <summary>
        /// 几何列，wkt字符串
        /// </summary>
        [Field("geometry", GeoFieldType.GEOMETRY)]
        public virtual string Geometry
        {
            get { return _geometry; }
            set { SetProperty(ref _geometry, value); }
        }
    }
}
