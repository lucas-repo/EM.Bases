﻿using System;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace EM.SQLites
{
    /// <summary>
    /// SQLite数据库上下文
    /// </summary>
    public abstract class SQLiteContext : IDisposable
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        public SQLiteConnection Connection { get; private set; }
        /// <summary>
        /// 数据库连接状态
        /// </summary>
        public ConnectionState ConnectionState
        {
            get
            {
                ConnectionState state = ConnectionState.Closed;
                if (Connection != null)
                {
                    state = Connection.State;
                }
                return state;
            }
        }

        public SQLiteContext(SQLiteConnection connection)
        {
            Connection = connection;
        }

        /// <summary>
        /// 创数据库
        /// </summary>
        /// <param name="filename">文件名</param>
        public static void CreateDatabase(string filename)
        {
            try
            {
                var directory = Path.GetDirectoryName(filename);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                SQLiteConnection.CreateFile(filename);//第一步，创建数据库
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(CreateDatabase)}_{filename}失败，{e}");
            }
        }

        /// <summary>
        /// 执行sql方法（自动打开数据库链接）
        /// </summary>
        /// <param name="action">要执行的sql查询方法</param>
        /// <param name="useTransaction">使用事务</param>
        /// <returns>任务</returns>
        public void Execute(Action action, bool useTransaction = false)
        {
            if (Connection == null || action == null)
            {
                return;
            }
            Connection.Execute(action, useTransaction);
        }
        /// <summary>
        /// 打开数据库连接
        /// </summary>
        /// <returns></returns>
        public virtual void OpenConnection()
        {
            if (Connection != null && Connection.State != ConnectionState.Open)
            {
                 Connection.Open();
            }
        }
        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public virtual void CloseConnection()
        {
            if (Connection != null && Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }
        /// <summary>
        /// 释放
        /// </summary>
        public void Dispose()
        {
            if (Connection != null)
            {
                CloseConnection();
                Connection.Dispose();
                Connection = null;
            }
        }

    }
}
