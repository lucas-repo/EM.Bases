using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM.SQLites
{
    /// <summary>
    /// 表信息（字段）
    /// </summary>
    [Serializable]
    public class TableInfo: Record
    {
        /// <summary>
        /// 列索引
        /// </summary>
        [Field("cid", FieldType.INTEGER)]
        public int ColumnId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [Field("name", FieldType.TEXT)]
        public string Name { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        [Field("type", FieldType.TEXT)]
        public string Type { get; set; }
        /// <summary>
        /// 是否不为空，0允许空值，其他则不允许空值
        /// </summary>
        [Field("notnull", FieldType.INTEGER)]
        public int Notnull { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        [Field("dflt_value")]
        public object DefaultValue { get; set; }
        /// <summary>
        /// 主键
        /// </summary>
        [Field("pk", FieldType.INTEGER)]
        public int PrimatyKey { get; set; }
    }
}
