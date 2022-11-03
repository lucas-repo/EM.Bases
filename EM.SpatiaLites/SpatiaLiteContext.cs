using EM.Bases;
using EM.SQLites;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EM.SpatiaLites
{
    /// <summary>
    /// 空间数据库上下文
    /// </summary>
    public class SpatiaLiteContext : SQLites.SQLiteContext
    {
        static SpatiaLiteContext()
        {
            var directory = Path.GetDirectoryName(typeof(SpatiaLiteContext).Assembly.Location);
        }
        public SpatiaLiteContext(SQLiteConnection connection) : base(connection)
        {
        }
        /// <summary>
        /// 创建空间数据库
        /// </summary>
        /// <param name="filename">路径</param>
        /// <returns>成功true反之false</returns>
        public static bool CreateSpatialDatabase(string filename)
        {
            var ret = typeof(SpatiaLiteContext).Assembly.CopyEmbeddedResourceToFile("annotation.db",filename);
            return ret;
        }
        /// <summary>
        /// 创建空间表
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnInfos">列信息集合</param>
        /// <param name="geometryColumn">几何列</param>
        /// <returns>任务</returns>
        public async Task CreateSpatialTable(string tableName, IEnumerable<TableInfo> columnInfos, GeometryColumnRecord geometryColumn)
        {
            if (Connection == null || string.IsNullOrEmpty(tableName) || !(columnInfos?.Count() > 0) || geometryColumn == null)
            {
                return;
            }
            var createTableSql = SpatiaLiteQueries.GetCreateSpatialTableSql(tableName, columnInfos);//第一步，创建空间表
            if (string.IsNullOrEmpty(createTableSql))
            {
                return;
            }
            var addGeometryColumnSql = geometryColumn.GetAddGeometryColumnSql();//第二步，添加几何列
            if (string.IsNullOrEmpty(addGeometryColumnSql))
            {
                return;
            }
            var createSpatialIndexSql = SpatiaLiteQueries.GetCreateSpatialIndexSql(tableName, geometryColumn.GeometryColumn);//第三步，创建空间索引
            if (string.IsNullOrEmpty(createSpatialIndexSql))
            {
                return;
            }
            var sql = $"{createTableSql}{addGeometryColumnSql}{createSpatialIndexSql}";
            var ret = await Connection.ExecuteNonQueryAsync(sql, useTransaction: true);
        }
        public override async Task OpenConnection()
        {
            await base.OpenConnection();//打开数据库连接
            if (Connection != null)
            {
                await Connection.LoadModeSpatialite();//打开数据库连接后必须加载空间扩展
            }
        }
    }
}
