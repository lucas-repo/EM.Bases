using EM.Bases;
using System.Windows;

namespace EM.WpfBases
{
    /// <summary>
    /// 视图模型
    /// </summary>
    /// <typeparam name="T">控件元素</typeparam>
    [Serializable]
    public class ViewModel<T> : NotifyClass where T : FrameworkElement
    {
        /// <summary>
        /// 视图
        /// </summary>
        [NonSerialized]
        public readonly T View;
        /// <summary>
        /// 实例化视图模型
        /// </summary>
        /// <param name="t">控件元素</param>
        public ViewModel(T t)
        {
            View = t??throw new ArgumentNullException(nameof(t));
        }
    }
}