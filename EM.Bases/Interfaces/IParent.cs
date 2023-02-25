using System;
using System.Collections.Generic;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 带父元素的接口
    /// </summary>
    /// <typeparam name="T">父元素类型</typeparam>
    public interface IParent<T>
    {
        /// <summary>
        /// 父元素
        /// </summary>
        T Parent { get; set; }
    }
}
