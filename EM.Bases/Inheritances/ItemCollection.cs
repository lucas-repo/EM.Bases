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
    public class ItemCollection<T> : NotifyClass, IItemCollection<T>
    {
        /// <summary>
        /// 元素集合
        /// </summary>
        protected ObservableCollection<T> Items { get; }
        /// <inheritdoc/>
        public int Count => Items.Count;
        /// <inheritdoc/>
        public bool IsReadOnly => false;
        /// <inheritdoc/>
        public T this[int index] { get => Items[index]; set => Items[index] = value; }

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        /// <summary>
        /// 实例化元素集合
        /// </summary>
        public ItemCollection()
        {
            Items = new ObservableCollection<T>();
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
        /// <inheritdoc/>
        public virtual IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
        /// <inheritdoc/>
        public virtual int IndexOf(T item)
        {
            return Items.IndexOf(item);
        }

        /// <inheritdoc/>
        public virtual void Insert(int index, T item)
        {
            Items.Insert(index, item);
        }

        /// <inheritdoc/>
        public virtual void RemoveAt(int index)
        {
            Items.RemoveAt(index);
        }

        /// <inheritdoc/>
        public virtual void Add(T item)
        {
            Items.Add(item);
        }

        /// <inheritdoc/>
        public virtual void Clear()
        {
            Items.Clear();
        }

        /// <inheritdoc/>
        public virtual bool Contains(T item)
        {
            return Items.Contains(item);
        }

        /// <inheritdoc/>
        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            Items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public virtual bool Remove(T item)
        {
            return Items.Remove(item);
        }
    }
    /// <summary>
    /// 元素集合
    /// </summary>
    public class ItemCollection : ItemCollection<IBaseItem>
    {
    }
}
