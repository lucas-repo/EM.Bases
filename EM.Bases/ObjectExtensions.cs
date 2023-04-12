using System;
using System.Reflection;

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
        /// <summary>
        /// 强制设置属性的值(数字之间可转换)
        /// </summary>
        /// <param name="propertyInfo">属性信息</param>
        /// <param name="destObj">目标对象</param>
        /// <param name="value">值</param>
        public static void ForceSetValue(this PropertyInfo propertyInfo, object destObj, object value)
        {
            if (propertyInfo == null || DBNull.Value.Equals(value) || value == null || destObj == null)
            {
                return;
            }
            var fieldType = value.GetType();
            var destType = propertyInfo.PropertyType;
            if (destType.Equals(fieldType))
            {
                propertyInfo.SetValue(destObj, value);
            }
            else
            {
                if (destType.IsEnum)
                {
                    var destValue = Enum.ToObject(destType, value);
                    propertyInfo.SetValue(destObj, destValue);
                }
                else
                {
                    bool isSuccess;
                    switch (destType.Name)
                    {
                        case "Int32":
                            isSuccess = value.TryGetInt(out var intValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, intValue);
                            }
                            break;
                        case "Int64":
                            isSuccess = value.TryGetLong(out var longValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, longValue);
                            }
                            break;
                        case "Double":
                            isSuccess = value.TryGetDouble(out var doubleValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, doubleValue);
                            }
                            break;
                        case "Single":
                            isSuccess = value.TryGetFloat(out var floatValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, floatValue);
                            }
                            break;
                        case "String":
                            isSuccess = value.TryGetString(out var stringValue);
                            if (isSuccess)
                            {
                                propertyInfo.SetValue(destObj, stringValue);
                            }
                            break;
                        default:
                            propertyInfo.SetValue(destObj, value);
                            break;
                    }
                }
            }
        }
    }
}
