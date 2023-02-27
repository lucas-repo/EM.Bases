using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EM.Bases
{
    /// <summary>
    /// 程序集扩展
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// 目录和程序集集合字典
        /// </summary>
        private static ConcurrentDictionary<string, IEnumerable<Assembly>> DirectoryAndAssemblies { get; } = new ConcurrentDictionary<string, IEnumerable<Assembly>>();
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

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="destType"></param>
        /// <returns></returns>
        public static object CreateInstance(Assembly assembly, Type destType)
        {
            object t = default;
            if (assembly != null && destType != null)
            {
                foreach (Type item in assembly.GetTypes())
                {
                    if (!item.IsAbstract && destType.IsAssignableFrom(item))
                    {
                        t = Activator.CreateInstance(item);
                        break;
                    }
                }
            }
            return t;
        }
        /// <summary>
        /// 从文件中读取dll创建实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static T CreateInstance<T>(string directory)
        {
            Type destType = typeof(T);
            T t = default;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                t = (T)CreateInstance(assembly, destType);
                if (t != null)
                {
                    break;
                }
            }
            if (t == null)
            {
                string[] dllFiles = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (string dllFileName in dllFiles)
                {
                    if (assemblies.Any(x => x.Location == dllFileName))
                    {
                        continue;
                    }
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(dllFileName);
                        t = (T)CreateInstance(assembly, destType);
                        if (t != null)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"CreateInterface Error!{ex}");
                    }
                }
            }
            return t;
        }
        /// <summary>
        /// 程序集是否可分配指定类型实例
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="destType"></param>
        /// <returns></returns>
        public static bool IsAssignable(Assembly assembly, Type destType)
        {
            bool ret = false;
            if (assembly != null && destType != null)
            {
                foreach (Type item in assembly.GetTypes())
                {
                    if (!item.IsAbstract && destType.IsAssignableFrom(item))
                    {
                        ret = true;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 获取指定名称的程序集
        /// </summary>
        /// <param name="directory">目录</param>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns>程序集</returns>
        public static Assembly GetAssembly(string directory, string assemblyName)
        {
            var knownExtensions = new[] { "dll", "exe" };
            if (Directory.Exists(directory))
            {
                foreach (string extension in knownExtensions)
                {
                    var potentialFiles = Directory.GetFiles(directory, assemblyName + "." + extension, SearchOption.AllDirectories);
                    if (potentialFiles.Length > 0)
                        return Assembly.LoadFrom(potentialFiles[0]);
                }
            }
            // assembly not found
            return null;
        }
        /// <summary>
        /// 获取可分配指定类型的程序集
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static Assembly GetAssignableAssembly<T>(string directory)
        {
            Assembly destAssembly = null;
            Type destType = typeof(T);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                if (IsAssignable(assembly, destType))
                {
                    destAssembly = assembly;
                    break;
                }
            }
            if (destAssembly == null)
            {
                string[] dllFiles = Directory.GetFiles(directory, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (string dllFileName in dllFiles)
                {
                    if (assemblies.Any(x => x.Location == dllFileName))
                    {
                        continue;
                    }
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(dllFileName);
                        if (IsAssignable(assembly, destType))
                        {
                            destAssembly = assembly;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Load Assembly Error!{ex}");
                    }
                }
            }
            return destAssembly;
        }
        /// <summary>
        /// 获取指定名称的程序集
        /// </summary>
        /// <param name="directory">目录</param>
        /// <param name="extensions">扩展</param>
        /// <returns>程序集</returns>
        private static IEnumerable<Assembly> GetAssemblies(this string directory, params string[] extensions)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                yield break;
            }
            IEnumerable<string> destExtensions;
            if (extensions != null)
            {
                destExtensions = extensions.Distinct();
            }
            else
            {
                destExtensions = new string[] { ".dll" };
            }
            foreach (string extension in destExtensions)
            {
                var files = Directory.GetFiles(directory, $"*{extension}", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = Assembly.LoadFrom(file);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"加载程序集{file}失败，{e}");
                    }
                    if (assembly != null)
                    {
                        yield return assembly;
                    }
                }
            }
        }
        /// <summary>
        /// 获取指定名称的程序集
        /// </summary>
        /// <param name="directory">目录</param>
        /// <param name="searchOption">查找模式</param>
        /// <returns>程序集</returns>
        public static IEnumerable<Assembly> GetAssemblies(this string directory, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                yield break;
            }
            if (DirectoryAndAssemblies.TryGetValue(directory, out var assemblies))
            {
                foreach (var item in assemblies)
                {
                    yield return item;
                }
            }
            else
            {
                var assemblies0 = directory.GetAssemblies(".dll");
                DirectoryAndAssemblies.TryAdd(directory, assemblies0);
                foreach (var item in assemblies0)
                {
                    yield return item;
                }
                switch (searchOption)
                {
                    case SearchOption.AllDirectories:
                        var directories = Directory.GetDirectories(directory);
                        foreach (var childDirectory in directories)
                        {
                            var childAssemblies = childDirectory.GetAssemblies(searchOption);
                            foreach (var item in childAssemblies)
                            {
                                yield return item;
                            }
                        }
                        break;
                }
            }
        }
        /// <summary>
        /// 获取类型集合
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="directory">目录</param>
        /// <param name="searchOption">查找模式</param>
        /// <returns>类型集合</returns>
        public static IEnumerable<Type> GetTypes<T>(this string directory, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                yield break;
            }
            var baseType = typeof(T);
            var assemblies = directory.GetAssemblies(searchOption);
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (!type.IsAbstract && baseType.IsAssignableFrom(type))
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
