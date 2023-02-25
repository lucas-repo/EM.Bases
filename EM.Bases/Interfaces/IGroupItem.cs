using System;
using System.Collections.Generic;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 组元素
    /// </summary>
    public interface IGroupItem : ISelectableItem
    {
        /// <summary>
        /// 子元素集合
        /// </summary>
        IItemCollection<IBaseItem> Children { get; }
    }
    /// <summary>
    /// 组元素
    /// </summary>
    /// <typeparam name="TItem">元素类型</typeparam>
    /// <typeparam name="TChildren">子元素类型</typeparam>
    public interface IGroupItem<TItem, TChildren> : ISelectableItem<TItem>
    {
        /// <summary>
        /// 子元素集合
        /// </summary>
        IItemCollection<TChildren> Children { get; }
    }
}
