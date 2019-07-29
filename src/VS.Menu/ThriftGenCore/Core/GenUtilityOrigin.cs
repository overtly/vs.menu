using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using VS.Menu.Helper;

namespace VS.Menu.ThriftGenCore
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class GenUtilityOrigin
    {
        /// <summary>
        /// 执行thrift -gen csharp .thrift 命令得到生成的.cs文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="output"></param>
        /// <param name="error"></param>
        /// <returns>
        /// 是否成功以最终是否生成gen-csharp文件为准 这里会out出输出和错误 
        /// </returns>
        public static FileInfo[] GenThrift(string filePath, string outRootDic, out string output, out string error)
        {
            var file = new FileInfo(filePath);
            var shortFileName = file.Name;
            ThriftGlobal.ResetGenResultFolder(shortFileName);
            if (filePath.IndexOf(" ") > -1)
                filePath = "\"" + filePath + "\"";
            var content = String.Concat("-o \"", outRootDic, "\" -r -gen csharp \"", filePath, "\"");
            Utility.InvokeProcess(ThriftGlobal.ThriftExePath, null, out output, out error, Utility.AppBaseDic, content);

            var basePath = Path.Combine(outRootDic, "gen-csharp");
            // 检测是否生成gen-csharp文件夹
            if (!Directory.Exists(basePath))
                return null;
            // 从gen-csharp文件夹中获取到所有的文件
            var childDics = Directory.GetDirectories(basePath);
            while (childDics.Length > 0)
            {
                basePath = childDics[0];
                childDics = Directory.GetDirectories(basePath);
            }
            // 由于要获取全路径所以返回file
            return new DirectoryInfo(basePath).GetFiles();
        }
    }
}