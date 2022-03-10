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
    public class TreeItem<T> : NotifyClass
    {
        private string _text = string.Empty;
        /// <summary>
        /// 显示文本
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
        private T _value;
        /// <summary>
        /// 值
        /// </summary>
        public T Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        /// <summary>
        /// 父节点
        /// </summary>
        public TreeItem<T> Parent { get; set; }
        /// <summary>
        /// 子节点集合
        /// </summary>
        public ObservableCollection<TreeItem<T>> Children { get; } = new ObservableCollection<TreeItem<T>>() { };
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
        public TreeItem(T value)
        {
            _value = value;
            _text =$"{value}";
        }
        public TreeItem(T value, string text)
        {
            _value = value;
            _text = text;
        }
        public override string ToString()
        {
            return Text;
        }
    }
}
