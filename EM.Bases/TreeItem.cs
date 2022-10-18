using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM.Bases
{
    /// <summary>
    /// 泛型树节点
    /// </summary>
    public class TreeItem<T> : SelectableItem<T>, ITreeItem<T>
    {
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
    }
}
