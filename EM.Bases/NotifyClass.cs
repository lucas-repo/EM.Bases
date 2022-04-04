using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EM.Bases
{
    /// <summary>
    /// 通知类
    /// </summary>
    [Serializable]
    public abstract class NotifyClass : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 设置值并调用属性改变通知
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="t">变量</param>
        /// <param name="value">值</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>成功true，反之false</returns>
        public bool SetProperty<T>(ref T t, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(t, value))
            {
                t = value;
                OnPropertyChanged(propertyName); 
                return true;
            }
            return false;
        }

        /// <summary>
        /// 属性更改方法
        /// </summary>
        /// <param name="propertyName">属性名</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
