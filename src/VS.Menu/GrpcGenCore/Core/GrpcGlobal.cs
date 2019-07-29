using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;
using VS.Menu.Model;
using System.Xml;
using VS.Menu.Helper;
using System.Linq;
using System.Threading;

namespace VS.Menu.GrpcGenCore
{
    public class GrpcGlobal
    {
        #region Static Property
        /// <summary>
        /// thrift 命令行生成文件的地址
        /// </summary>
        public static string GenFolder
        {
            get { return Path.Combine(Utility.AppBaseDic, "grpc-gen"); }
        }

        /// <summary>
        /// grpc-complie文件夹
        /// </summary>
        public static string BuildFolder
        {
            get { return Path.Combine(Utility.AppBaseDic, "grpc-build"); }
        }

        /// <summary>
        /// grpc-complie文件夹
        /// </summary>
        public static string ResultFolder
        {
            get { return Path.Combine(Utility.AppBaseDic, "grpc-complie"); }
        }

        public static string AssemblyFolder
        {
            get { return Path.Combine(Utility.AppBaseDic, "grpc-complie"); }
        }

        /// <summary>
        /// proto
        /// </summary>
        public static string ProtoExePath
        {
            get
            {
                var path = Path.Combine(Utility.AppBaseResourceDic, "proto.exe");
                Utility.ExportFile(path, Resources.protoc);

                return path;
            }
        }
        /// <summary>
        /// grpc_csharp_plugin
        /// </summary>
        public static string CsharpPluginsPath
        {
            get
            {
                var path = Path.Combine(Utility.AppBaseResourceDic, "grpc_csharp_plugin.exe");
                Utility.ExportFile(path, Resources.grpc_csharp_plugin);

                return path;
            }
        }
        /// <summary>
        /// project.assets.json
        /// </summary>
        public static string ProjectAssetPath
        {
            get
            {
                var path = Path.Combine(Utility.AppBaseResourceDic, "project.assets.json");
                Utility.ExportFile(path, Resources.project_assets);
                return path;
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// 返回生成结果的文件夹
        /// </summary>
        /// <param name="shortThriftFileName"></param>
        /// <returns></returns>
        public static string GenResultDic(string shortFileName)
        {
            var dicPath = Path.Combine(ResultFolder,
                string.Format("result{0}", shortFileName));
            // 我不能保证MSBuild的生成项目这一步没有错
            // 如果有错,则错误是不会throw到客户端的
            // 所以我每次都删除这个文件夹重新创建
            Utility.DelDir(dicPath);
            System.Threading.Thread.Sleep(20);
            Utility.MakeDir(dicPath);
            return dicPath;
        }

        /// <summary>
        /// 根据.thrift文件创建的生成文件的目录
        /// </summary>
        /// <param name="shortFileName"></param>
        /// <returns>该目录的路径</returns>
        public static string GenProjDic(string shortFileName)
        {
            System.Threading.Thread.Sleep(20);
            var dicPath = GetProjDic(shortFileName);
            Utility.DelDir(dicPath);
            System.Threading.Thread.Sleep(20);
            Utility.MakeDir(dicPath);
            return dicPath;
        }

        /// <summary>
        /// 创建 obj文件夹
        /// </summary>
        /// <param name="shortFileName"></param>
        /// <returns></returns>
        public static string GenProjObjDic(string shortFileName)
        {
            var dicPath = Path.Combine(GetProjDic(shortFileName), "obj");
            Utility.DelDir(dicPath);
            System.Threading.Thread.Sleep(20);
            Utility.MakeDir(dicPath);
            return dicPath;
        }

        /// <summary>
        /// 移动配置文件
        /// </summary>
        /// <param name="shortFileName"></param>
        public static void MoveProjectAsset(string shortFileName)
        {
            var dicPath = GenProjObjDic(shortFileName);
            var fileInfo = new FileInfo(ProjectAssetPath);
            var fileName = fileInfo.Name;
            var desPath = Path.Combine(dicPath, fileName);
            File.Copy(ProjectAssetPath, desPath, true);

            do
            {
                Thread.Sleep(100);
                if (File.Exists(desPath))
                    break;
            } while (true);

            var desFileInfo = new FileInfo(desPath);

            // 修改用户
            var userName = Environment.UserName;
            if (userName != "Administrator")
            {
                var json = string.Empty;
                using (var fs = desFileInfo.OpenText())
                    json = fs.ReadToEnd();

                json = json.Replace("Administrator", userName);
                var bts = Encoding.UTF8.GetBytes(json);
                using (var fs = desFileInfo.OpenWrite())
                {
                    fs.Write(bts, 0, bts.Count());
                }
            }
            Thread.Sleep(20);
        }

        /// <summary>
        /// ProjDic
        /// </summary>
        /// <param name="shortFileName"></param>
        /// <returns></returns>
        public static string GetProjDic(string shortFileName)
        {
            var dicPath = Path.Combine(BuildFolder,
                string.Format("build{0}Proj", shortFileName));
            return dicPath;
        }

        /// <summary>
        /// 复制地址
        /// </summary>
        /// <param name="shortFileName"></param>
        /// <returns></returns>
        public static string GetLoadAssemblyDic(string shortFileName, string tempKey = "")
        {
            if (string.IsNullOrEmpty(tempKey))
                tempKey = "temp";
            var dicPath = Path.Combine(AssemblyFolder, string.Format("ass{0}Proj", shortFileName));

            Utility.DelDir(dicPath);
            System.Threading.Thread.Sleep(20);
            Utility.MakeDir(dicPath);

            dicPath = Path.Combine(dicPath, tempKey);
            Utility.MakeDir(dicPath);

            return dicPath;
        }

        /// <summary>
        /// 通过注册表获取MSBuild的路径
        /// 如果失败,则默认组装X86 .net framework 4.0的地址
        /// </summary>
        public static string MsBuildPath()
        {
            var keyPath = @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\MuiCache";
            var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);

            var subKeys = key.OpenSubKey(keyPath);
            if (subKeys == null)
                return string.Empty;

            var values = subKeys.GetValueNames()
                .Where(oo => oo.Contains(@"2017") && oo.Contains(@"Common7\IDE\devenv.exe")).ToList();
            if (values.Count() <= 0)
            {
                return string.Empty;
            }

            var existMsBuildPaths = new List<string>();
            foreach (var item in values)
            {
                var msBuildPath = item.Replace(".FriendlyAppName", "").Replace(@"Common7\IDE\devenv.exe", @"MSBuild\15.0\Bin\amd64\MSBuild.exe");
                if (File.Exists(msBuildPath))
                    existMsBuildPaths.Add(msBuildPath);
            }

            if (existMsBuildPaths.Any(oo => oo.Contains("Enterprise")))
                return existMsBuildPaths.FirstOrDefault(oo => oo.Contains("Enterprise"));

            return existMsBuildPaths.FirstOrDefault();
        }
        #endregion
    }
}