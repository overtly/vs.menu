using VS.Menu.Helper;
using VS.Menu.ThriftGenCore;
using VS.Menu.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace VS.Menu.Helper
{
    /// <summary>
    /// 保存在客户端本地C盘下的配置文件
    /// 方便用户不用每次更新后都要重新设置
    /// </summary>
    public class VersionHelper
    {
        private static readonly string versionPath = Path.Combine(Utility.AppBaseDic, "thrift4GenServiceTool.version");

        /// <summary>
        /// 保存thrift模板文件的文件夹路径
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static bool Compare(string curVersion)
        {
            if (!File.Exists(versionPath))
            {
                File.WriteAllText(versionPath, curVersion);
                return false;
            }

            var version = File.ReadAllText(versionPath);
            if (curVersion != version)
            {
                File.WriteAllText(versionPath, curVersion);
                return false;
            }
            return true;
        }
    }
}