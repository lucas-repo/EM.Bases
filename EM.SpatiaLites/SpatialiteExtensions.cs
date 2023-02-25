using EM.SQLites;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EM.SpatiaLites
{
    public static class SpatialiteExtensions
    {
        /// <summary>
        /// 添加空间扩展
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <exception cref="Exception">加载扩展异常</exception>
        public static void LoadModeSpatialite(this SQLiteConnection connection)
        {
            if (connection == null)
            {
                return;
            }
            connection.Execute( () =>
           {
               try
               {
                   connection.LoadExtension("mod_spatialite");
               }
               catch (Exception e)
               {
                   throw new Exception($"{nameof(LoadModeSpatialite)}失败，请将mod_spatialite相关文件放至程序根目录，或将目录添加至注册表Path。{e}");
               }
           });
        }
    }
}
