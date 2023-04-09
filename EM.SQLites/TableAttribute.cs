using System;
using System.Collections.Generic;
using System.Text;

namespace EM.SQLites
{
    /// <summary>
    /// 表特性
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class TableAttribute:Attribute
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; }
        public TableAttribute(string name)
        {
            Name = name;
        }
    }
}
