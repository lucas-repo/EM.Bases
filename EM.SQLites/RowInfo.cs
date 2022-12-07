using System;
using System.Collections.Generic;
using System.Text;

namespace EM.SQLites
{
    /// <summary>
    /// 数据行信息
    /// </summary>
    public class RowInfo
    {
        /// <summary>
        /// 数据行的id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 字段和值集合
        /// </summary>
        public Dictionary<TableInfo, object> FieldAndValues { get; } 
        public RowInfo(string id, Dictionary<TableInfo, object> fieldAndValues)
        {
            Id = id;
            FieldAndValues = fieldAndValues;
        }
        public RowInfo()
        {
            FieldAndValues = new Dictionary<TableInfo, object>();
        }
    }
}
