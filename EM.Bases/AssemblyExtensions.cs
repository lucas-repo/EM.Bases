using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 程序集扩展
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// 拷贝嵌入资源至文件
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="resourceName">资源名</param>
        /// <param name="destFilename">文件路径</param>
        /// <returns>成功true反之false</returns>
        public static bool CopyEmbeddedResourceToFile(this Assembly assembly, string resourceName, string destFilename)
        {
            bool ret = false;
            if (assembly == null || string.IsNullOrEmpty(resourceName))
            {
                return ret;
            }
            var assemblyName = assembly.GetName().Name;
            var destName = $"{assemblyName}.{resourceName}";
            var stream = assembly.GetManifestResourceStream(destName);
            if (stream != null)
            {
                using (var fileStream = File.Open(destFilename, FileMode.CreateNew, FileAccess.ReadWrite))
                {
                    stream.CopyTo(fileStream);
                    fileStream.Close();
                }
                stream.Seek(0, SeekOrigin.Begin);
                ret = true;
            }
            return ret;
        }
    }
}
