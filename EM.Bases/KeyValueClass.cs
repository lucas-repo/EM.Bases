using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM.Bases
{
    /// <summary>
    /// 键值类
    /// </summary>
    public class KeyValueClass<TKey,TValue>:NotifyClass
    {
        private TKey _key;
        /// <summary>
        /// 键
        /// </summary>
        public TKey Key
        {
            get { return _key; }
            set { SetProperty(ref _key, value); }
        }
        private TValue _value;

        /// <summary>
        /// 值
        /// </summary>
        public TValue Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public KeyValueClass(TKey key, TValue value)
        {
            _key=key;
            _value=value;
        }
    }
}
