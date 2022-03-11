using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EM.Bases
{
    /// <summary>
    /// 泛型委托命令
    /// </summary>
    /// <typeparam name="T">泛型</typeparam>
    [Serializable]
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;
        /// <summary>
        /// 能否执行改变事件
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// 实例化委托命令
        /// </summary>
        /// <param name="execute">执行委托</param>
        public DelegateCommand(Action execute) : this(p => execute?.Invoke()) { }

        /// <summary>
        /// 实例化委托命令
        /// </summary>
        /// <param name="execute">执行委托</param>
        public DelegateCommand(Action<T> execute) 
        {
            _execute = execute??throw new ArgumentNullException(nameof(execute));
        }
        /// <summary>
        /// 实例化委托命令
        /// </summary>
        /// <param name="execute">执行委托</param>
        /// <param name="canExecute">能否执行委托</param>
        public DelegateCommand(Action execute, Func<bool> canExecute): this(p => execute?.Invoke(), p => canExecute())
        { }
        /// <summary>
        /// 实例化委托命令
        /// </summary>
        /// <param name="execute">执行委托</param>
        /// <param name="canExecute">能否执行委托</param>
        /// <exception cref="ArgumentNullException"></exception>
        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute??throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 判断是否能执行
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <returns>返回是或否</returns>
        public bool CanExecute(object parameter)
        {
            bool ret = false;
            if (_canExecute==null)
            {
                ret = true;
            }
            else
            {
                ret = _canExecute((T)parameter);
            }
            return ret;
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="parameter">参数</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Execute(object parameter)
        {
            _execute?.Invoke((T)parameter);
        }

        /// <summary>
        /// 触发执行改变事件
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }
        /// <summary>
        /// 触发执行改变事件
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            CanExecuteChanged?.Invoke(this, e);
        }
    }
    /// <summary>
    /// 委托命令
    /// </summary>
    public class DelegateCommand : DelegateCommand<object>
    {
        /// <summary>
        /// 实例化委托命令
        /// </summary>
        /// <param name="execute">执行委托</param>
        public DelegateCommand(Action execute) : base(execute)
        {
        }
        /// <summary>
        /// 实例化委托命令
        /// </summary>
        /// <param name="execute">执行委托</param>
        public DelegateCommand(Action<object> execute) : base(execute)
        {
        }
        /// <summary>
        /// 实例化委托命令
        /// </summary>
        /// <param name="execute">执行委托</param>
        /// <param name="canExecute">是否可执行</param>
        public DelegateCommand(Action execute, Func<bool> canExecute) : base(execute, canExecute)
        {
        }
        /// <summary>
        /// 实例化委托命令
        /// </summary>
        /// <param name="execute">执行委托</param>
        /// <param name="canExecute">是否可执行</param>
        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute) : base(execute, canExecute)
        {
        }
    }
}
