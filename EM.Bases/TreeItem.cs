using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM.Bases
{
    /// <summary>
    /// 树节点
    /// </summary>
    public class TreeItem : BaseCopy, ITreeItem
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
        private ITreeItem _parent;
        /// <summary>
        /// 父节点
        /// </summary>
        public virtual ITreeItem Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        /// <summary>
        /// 子节点集合
        /// </summary>
        public IItemCollection<ITreeItem> Children { get; } = new ItemCollection<ITreeItem>() { };
        /// <summary>
        /// 级别
        /// </summary>
        public virtual int Level
        {
            get
            {
                int level = 0;
                var parent = Parent;
                while (parent!=null)
                {
                    level++;
                    parent=parent.Parent;
                }
                return level;
            }
        }

        private bool _isVisible;
        public virtual bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        private bool _isExpanded = true;
        public virtual bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
        private bool _isSelected;
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
        public override string ToString()
        {
            return Text;
        }
    }
    /// <summary>
    /// 泛型树节点
    /// </summary>
    public class TreeItem<T> : BaseCopy, ITreeItem<T>
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
        private T _item;
        /// <summary>
        /// 节点元素
        /// </summary>
        public virtual T Item
        {
            get { return _item; }
            set { SetProperty(ref _item, value); }
        }
        private ITreeItem<T> _parent;
        /// <summary>
        /// 父节点
        /// </summary>
        public virtual ITreeItem<T> Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        /// <summary>
        /// 子节点集合
        /// </summary>
        public IItemCollection<ITreeItem<T>> Children { get; } = new ItemCollection<ITreeItem<T>>() { };
        /// <summary>
        /// 级别
        /// </summary>
        public virtual int Level
        {
            get
            {
                int level = 0;
                var parent = Parent;
                while (parent!=null)
                {
                    level++;
                    parent=parent.Parent;
                }
                return level;
            }
        }

        private bool _isVisible;
        public virtual bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        private bool _isExpanded = true;
        public virtual bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
        private bool _isSelected;
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }
        public TreeItem(T item)
        {
            _item = item;
            _text =$"{item}";
        }
        public TreeItem(T item, string text)
        {
            _item = item;
            _text = text;
        }
        public override string ToString()
        {
            return Text;
        }
    }
}
