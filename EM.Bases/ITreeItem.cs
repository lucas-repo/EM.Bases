using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 树节点接口
    /// </summary>
    public interface ITreeItem
    {
        /// <summary>
        /// 显示文本
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// 是否展开
        /// </summary>
        bool IsExpanded { get; set; }
        /// <summary>
        /// 是否选择
        /// </summary>
        bool IsSelected { get; set; }
        /// <summary>
        /// 是否可见
        /// </summary>
        bool IsVisible { get; set; }
        /// <summary>
        /// 父节点
        /// </summary>
        ITreeItem Parent { get; set; }
        /// <summary>
        /// 子节点集合
        /// </summary>
        IItemCollection<ITreeItem> Children { get; }
        /// <summary>
        /// 级别
        /// </summary>
        int Level { get; }
    }
    /// <summary>
    /// 树节点接口
    /// </summary>
    public interface ITreeItem<T>
    {
        /// <summary>
        /// 显示文本
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// 节点元素
        /// </summary>
        T Item { get; set; }
        /// <summary>
        /// 是否展开
        /// </summary>
        bool IsExpanded { get; set; }
        /// <summary>
        /// 是否选择
        /// </summary>
        bool IsSelected { get; set; }
        /// <summary>
        /// 是否可见
        /// </summary>
        bool IsVisible { get; set; }
        /// <summary>
        /// 父节点
        /// </summary>
        ITreeItem<T> Parent { get; set; }
        /// <summary>
        /// 子节点集合
        /// </summary>
        IItemCollection<ITreeItem<T>> Children { get; }
        /// <summary>
        /// 级别
        /// </summary>
        int Level { get; }
    }
  
}
