using System;
using System.Collections.Generic;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 树节点信息接口
    /// </summary>
    public  interface ItreeItemInfo
    {
        /// <summary>
        /// 是否展开
        /// </summary>
        bool IsExpanded { get; set; }
        /// <summary>
        /// 是否可见
        /// </summary>
        bool IsVisible { get; set; }
        /// <summary>
        /// 级别
        /// </summary>
        int Level { get; }
    }
}
