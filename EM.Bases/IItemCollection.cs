using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 可通知的集合
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    public interface IItemCollection<T> : IEnumerable<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {

    }
}
