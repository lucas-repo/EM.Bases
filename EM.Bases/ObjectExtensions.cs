using System;

namespace EM.Bases
{
    /// <summary>
    /// 对象扩展
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="value">值</param>
        /// <returns>成功true反之false</returns>
        public static bool TryGetInt(this object obj, out int value)
        {
            bool ret = false;
            value = 0;
            if (obj == null || DBNull.Value.Equals(obj))
            {
                return ret;
            }
            if (obj is int destValue)
            {
                value = destValue;
                ret = true;
            }
            else
            {
                var type = obj.GetType();
                switch (type.Name)
                {
                    case "Int16":
                    case "Int64":
                    case "Single":
                    case "Double":
                        value = Convert.ToInt32(obj);
                        ret = true;
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="value">值</param>
        /// <returns>成功true反之false</returns>
        public static bool TryGetLong(this object obj, out long value)
        {
            bool ret = false;
            value = 0;
            if (obj == null || DBNull.Value.Equals(obj))
            {
                return ret;
            }
            if (obj is long destValue)
            {
                value = destValue;
                ret = true;
            }
            else
            {
                var type = obj.GetType();
                switch (type.Name)
                {
                    case "Int16":
                    case "Int32":
                    case "Single":
                    case "Double":
                        value = Convert.ToInt64(obj);
                        ret = true;
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="value">值</param>
        /// <returns>成功true反之false</returns>
        public static bool TryGetFloat(this object obj, out float value)
        {
            bool ret = false;
            value = 0;
            if (obj == null || DBNull.Value.Equals(obj))
            {
                return ret;
            }
            if (obj is float destValue)
            {
                value = destValue;
                ret = true;
            }
            else
            {
                var type = obj.GetType();
                switch (type.Name)
                {
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Double":
                        value = Convert.ToSingle(obj);
                        ret = true;
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="value">值</param>
        /// <returns>成功true反之false</returns>
        public static bool TryGetDouble(this object obj, out double value)
        {
            bool ret = false;
            value = 0;
            if (obj == null || DBNull.Value.Equals(obj))
            {
                return ret;
            }
            if (obj is double destValue)
            {
                value = destValue;
                ret = true;
            }
            else
            {
                var type = obj.GetType();
                switch (type.Name)
                {
                    case "Int16":
                    case "Int32":
                    case "Int64":
                    case "Single":
                        value = Convert.ToDouble(obj);
                        ret = true;
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 尝试获取值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="value">值</param>
        /// <returns>成功true反之false</returns>
        public static bool TryGetString(this object obj, out string value)
        {
            bool ret = false;
            value = string.Empty;
            if (obj == null || DBNull.Value.Equals(obj))
            {
                return ret;
            }
            if (obj is string destValue)
            {
                value = destValue;
                ret = true;
            }
            else
            {
                value = obj.ToString();
            }
            return ret;
        }
    }
}
