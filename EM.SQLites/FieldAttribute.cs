using System;

namespace EM.SQLites
{
    /// <summary>
    /// 匹配数据库字段的特性
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class FieldAttribute : Attribute
    {
        /// <summary>
        /// 表信息
        /// </summary>
        public TableInfo TableInfo { get; }
        /// <summary>
        /// 初始化列
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="type">类型</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="notNull">是否不为空，0允许空值，其他则不允许空值</param>
        /// <param name="primaryKey">主键</param>
        public FieldAttribute(string name, string type = FieldType.TEXT, object defaultValue = null, int notNull = 0, int primaryKey = 0)
        {
            TableInfo = new TableInfo()
            {
                Name = name,
                Type = type,
                DefaultValue = defaultValue,
                Notnull = notNull,
                PrimatyKey = primaryKey
            };
        }
    }
}
