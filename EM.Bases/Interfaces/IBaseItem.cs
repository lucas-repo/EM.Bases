using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 基础元素
    /// </summary>
    public interface IBaseItem : INotifyPropertyChanged, ICloneable
    {
        /// <summary>
        /// 显示文本
        /// </summary>
        string Text { get; set; }
    }
    /// <summary>
    /// 泛型基础元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public interface IBaseItem<T> : IBaseItem
    {
        /// <summary>
        /// 元素
        /// </summary>
        T Item { get; set; }
    }
}
