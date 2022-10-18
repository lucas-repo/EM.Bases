using System;
using System.Collections.Generic;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 可选择的项
    /// </summary>
    public class SelectableItem<T>:BaseCopy
    {
        private string _text = string.Empty;
        /// <summary>
        /// 显示文本
        /// </summary>
        public virtual string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
        private bool _isSelected;
        /// <summary>
        /// 是否已选择
        /// </summary>
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
        private T _item;
        /// <summary>
        /// 元素
        /// </summary>
        public T Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
