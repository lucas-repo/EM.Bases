using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 元素集合
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    public class ItemCollection<T> :NotifyClass, IItemCollection<T>
    {
        /// <summary>
        /// 元素集合
        /// </summary>
        protected ObservableCollection<T> Items { get; } 
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public ItemCollection()
        {
            Items = new ObservableCollection<T>();
            Items.CollectionChanged+=Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}
