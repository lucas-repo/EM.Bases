namespace EM.SpatiaLites
{
    /// <summary>
    /// 坐标维度
    /// </summary>
    public class CoordDimension
    {
        /// <summary>
        /// 二维
        /// </summary>
        public const int XY = 2;
        /// <summary>
        /// 三维XYZ
        /// </summary>
        public const int XYZ = 3;
        /// <summary>
        /// 二维带M值
        /// </summary>
        public const int XYM = 3;
        /// <summary>
        /// 三维带M值
        /// </summary>
        public const int XYZM = 4;
        /// <summary>
        /// 返回坐标维度字符串
        /// </summary>
        /// <param name="coordDimension">坐标维度</param>
        /// <param name="geometryType">几何类型</param>
        /// <returns>坐标维度字符串</returns>
        public static string GetCoordDimensionString(int coordDimension, int geometryType)
        {
            string ret = string.Empty;
            switch (coordDimension)
            {
                case 2:
                    ret = "XY";
                    break;
                case 3:
                    int intValue = geometryType / 1000;
                    switch (intValue)
                    {
                        case 1:
                            ret = "XYZ";
                            break;
                        case 2:
                            ret = "XYM";
                            break;
                    }
                    break;
                case 4:
                    ret = "XYZM";
                    break;
            }
            return ret;
        }
    }
}