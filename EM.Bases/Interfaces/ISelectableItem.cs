using System;
using System.ComponentModel;

namespace EM.Bases
{
    /// <summary>
    /// 可选择项的接口
    /// </summary>
    public interface ISelectableItem : IBaseItem
    {
        /// <summary>
        /// 是否已选择
        /// </summary>
        bool IsSelected { get; set; }
    }
    /// <summary>
    /// 泛型可选择项的接口
    /// </summary>
    /// <typeparam name="T">指定类型</typeparam>
    public interface ISelectableItem<T>: IBaseItem<T>,ISelectableItem
    {
    }
}