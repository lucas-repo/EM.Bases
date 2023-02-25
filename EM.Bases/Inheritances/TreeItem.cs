using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM.Bases
{
    /// <summary>
    /// 泛型树元素
    /// </summary>
    public class TreeItem: GroupItem, ITreeItem
    {
        private ITreeItem _parent;
        /// <inheritdoc/>
        public virtual ITreeItem Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        /// <inheritdoc/>
        public virtual int Level
        {
            get
            {
                int level = 0;
                var parent = Parent;
                while (parent != null)
                {
                    level++;
                    parent = parent.Parent; 
                }
                return level;
            }
        }

        private bool _isVisible;
        /// <inheritdoc/>
        public virtual bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        private bool _isExpanded = true;
        /// <inheritdoc/>
        public virtual bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }
    }
}
