using EM.Bases;
using System;

namespace EM.SQLites
{
    /// <summary>
    /// 带id的行
    /// </summary>
    [Serializable]
    public class IdRecord : Record
    {
        private string _id;
        /// <summary>
        /// id
        /// </summary>
        [Field("id", FieldType.TEXT, notNull: 1, primaryKey: 1)]
        public string ID
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }
    }
}
