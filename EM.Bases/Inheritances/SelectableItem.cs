using System;
using System.Collections.Generic;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 泛型可选择的元素
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public class SelectableItem<T> : SelectableItem, ISelectableItem<T>
    {
        private T _item;
        /// <summary>
        /// 元素
        /// </summary>
        public virtual T Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Text;
        }
    }

    /// <summary>
    /// 可选择的元素
    /// </summary>
    public class SelectableItem: BaseCopy, ISelectableItem
    {
        private string _text = string.Empty;
        /// <inheritdoc/>
        public virtual string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
        private bool _isSelected;
        /// <inheritdoc/>
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
        /// <inheritdoc/>
        public override string ToString()
        {
            return Text;
        }
    }
}
