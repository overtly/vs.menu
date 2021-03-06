﻿using VS.Menu.Helper;
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
    public class TestHelper
    {
        private static readonly string debugPath = Path.Combine(Utility.AppBaseDic, "debug.log");

        /// <summary>
        /// 保存thrift模板文件的文件夹路径
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static void Write(string msg)
        {
            File.WriteAllText(debugPath, msg);
            if (!File.Exists(debugPath))
            {
                File.WriteAllText(debugPath, msg);
                return;
            }

            using (var sw = new StreamWriter(debugPath))
            {
                sw.WriteLine(msg);
            }
        }
    }
}