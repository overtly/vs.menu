using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32;
using VS.Menu.Model;
using System.Xml;
using VS.Menu.Helper;
using System.Threading;

namespace VS.Menu.ThriftGenCore
{
    public class ThriftGlobal
    {
        #region Static Property
        /// <summary>
        /// 生成的Thrift的版本
        /// </summary>
        public static EnumGenAsyncVersion GenAsyncVersion = EnumGenAsyncVersion.New;

        /// <summary>
        /// Thrift版本名称
        /// </summary>
        public static string ThriftVersion
        {
            get
            {
                if (GenAsyncVersion == EnumGenAsyncVersion.New)
                    return "";
                return "_1.0";
            }
        }

        /// <summary>
        /// thrift 命令行生成文件的地址
        /// </summary>
        public static string GenFolder
        {
            get
            {
                var fold = Path.Combine(Utility.AppBaseDic, "thrift-gen");
                if (GenAsyncVersion == EnumGenAsyncVersion.Old)
                    fold += "-old";
                return fold;
            }
        }

        /// <summary>
        /// 最终的结果文件夹
        /// </summary>
        public static string ResultFolder
        {
            get
            {
                var fold = Path.Combine(Utility.AppBaseDic, "thrift-complie");
                if (GenAsyncVersion == EnumGenAsyncVersion.Old)
                    fold += "-old";
                return fold;
            }
        }


        #region 生成Dll依赖的Thrift文件
        /// <summary>
        /// 执行的thrift.exe的路径
        /// </summary>
        public static string ThriftExePath
        {
            get
            {
                var path = Path.Combine(Utility.AppBaseResourceDic, "thrift.exe");

                Utility.ExportFile(path, Resources.ThriftExe);

                return path;
            }
        }

        /// <summary>
        /// thrift.dll的路径 作为内容生成在目录下的
        /// </summary>
        public static string ThriftDllPath
        {
            get
            {
                var path = string.Empty;
                if (GenAsyncVersion == EnumGenAsyncVersion.New)
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "Thrift.dll");
                    Utility.ExportFile(path, Resources.Thrift);
                }
                else
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "Thrift_1.0.dll");
                    Utility.ExportFile(path, Resources.Thrift_1_0);
                }

                return path;
            }
        }
        #endregion

        #region 生成Dll依赖的FaskSocketDll
        /// <summary>
        /// FastSocketClient
        /// </summary>
        public static string FastSocketClientDllPath
        {
            get
            {
                var path = string.Empty;
                if (GenAsyncVersion == EnumGenAsyncVersion.New)
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "FastSocket.Client.dll");
                    Utility.ExportFile(path, Resources.FastSocket_Client);
                }
                else
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "FastSocket.Client_1.0.dll");
                    Utility.ExportFile(path, Resources.FastSocket_Client_1_0);
                }

                return path;
            }
        }

        /// <summary>
        /// FastSocketClient
        /// </summary>
        public static string FastSocketSocketBaseDllPath
        {
            get
            {
                var path = string.Empty;
                if (GenAsyncVersion == EnumGenAsyncVersion.New)
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "FastSocket.SocketBase.dll");
                    Utility.ExportFile(path, Resources.FastSocket_SocketBase);
                }
                else
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "FastSocket.SocketBase_1.0.dll");
                    Utility.ExportFile(path, Resources.FastSocket_SocketBase_1_0);
                }

                return path;
            }
        }

        /// <summary>
        /// FastSocketClient
        /// </summary>
        public static string ThriftClientDllPath
        {
            get
            {
                var path = string.Empty;
                if (GenAsyncVersion == EnumGenAsyncVersion.New)
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "Thrift.Client.dll");
                    Utility.ExportFile(path, Resources.Thrift_Client);
                }
                else
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "Thrift.Client_1.0.dll");
                    Utility.ExportFile(path, Resources.Thrift_Client_1_0);
                }

                return path;
            }
        }

        /// <summary>
        /// FastSocketClient
        /// </summary>
        public static string ZookeeperDllPath
        {
            get
            {
                var path = string.Empty;
                if (GenAsyncVersion == EnumGenAsyncVersion.New)
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "Zookeeper.dll");
                    Utility.ExportFile(path, Resources.Zookeeper);
                }
                else
                {
                    path = Path.Combine(Utility.AppBaseResourceDic, "Zookeeper_1.0.dll");
                    Utility.ExportFile(path, Resources.Zookeeper);
                }

                return path;
            }
        }
        #endregion


        #endregion

        #region Public Methods
        /// <summary>
        /// 返回生成结果的文件夹
        /// </summary>
        /// <param name="shortThriftFileName"></param>
        /// <returns></returns>
        public static string GenResultDic(string shortThriftFileName)
        {
            var dicPath = Path.Combine(GenFolder,
                string.Format("gen{0}", shortThriftFileName));
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
        /// <param name="shortThriftFileName"></param>
        /// <returns>该目录的路径</returns>
        public static string GenProjDic(string shortThriftFileName)
        {
            var dicPath = GetProjDir(shortThriftFileName);
            // 我不能保证MSBuild的生成项目这一步没有错
            // 如果有错,则错误是不会throw到客户端的
            // 所以我每次都删除这个文件夹重新创建
            Utility.DelDir(dicPath);
            System.Threading.Thread.Sleep(20);
            Utility.MakeDir(dicPath);
            return dicPath;
        }

        /// <summary>
        /// ProjDic
        /// </summary>
        /// <param name="shortThriftFileName"></param>
        /// <returns></returns>
        public static string GetProjDir(string shortThriftFileName)
        {
            var dicPath = Path.Combine(ResultFolder,
                string.Format("build{0}Proj", shortThriftFileName));
            return dicPath;
        }

        /// <summary>
        /// 根据.thrift文件创建的生成文件的目录
        /// </summary>
        /// <param name="shortThriftFileName"></param>
        /// <returns>该目录的路径</returns>
        public static string GenServerProjDir(string shortThriftFileName)
        {
            var dicPath = Path.Combine(ResultFolder,
                string.Format("build{0}ServerProj", shortThriftFileName));
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
        /// <param name="shortThriftFileName"></param>
        /// <returns>该目录的路径</returns>
        public static string GenClientProjDir(string shortThriftFileName)
        {
            var dicPath = Path.Combine(ResultFolder,
                string.Format("build{0}ClientProj", shortThriftFileName));
            // 我不能保证MSBuild的生成项目这一步没有错
            // 如果有错,则错误是不会throw到客户端的
            // 所以我每次都删除这个文件夹重新创建
            Utility.DelDir(dicPath);
            System.Threading.Thread.Sleep(20);
            Utility.MakeDir(dicPath);
            return dicPath;
        }

        /// <summary>
        /// 制作AppConfig
        /// </summary>
        /// <param name="model"></param>
        public static void ChangeAppConfig(ServiceModel model, string appPath, List<FileInfo> otherConfigs)
        {
            var xDoc = new XmlDocument();
            xDoc.Load(appPath);//加载xml文件
            var servicesElem = (XmlElement)xDoc.SelectSingleNode("//services");

            if (otherConfigs.Count <= 0)
            {
                var xElem1 = (XmlElement)xDoc.SelectSingleNode("//service");//获取指定的xml子节点
                //var xElem2 = (XmlElement)xDoc.SelectSingleNode("//server");
                if (xElem1 != null)
                    xElem1.SetAttribute("name", model.ConfigServiceName);
                //if (xElem2 != null)
                //    xElem2.SetAttribute("port", model.Port.ToString());
            }

            foreach (var item in otherConfigs)
            {
                var otherDoc = new XmlDocument();
                otherDoc.Load(item.FullName);
                var toherServiceElem = (XmlElement)otherDoc.SelectSingleNode("//service");
                if (toherServiceElem == null)
                    continue;

                servicesElem.AppendChild(xDoc.ImportNode(toherServiceElem, true));
            }

            // 更改端口号
            var xNodes = servicesElem.SelectNodes("//server");
            foreach (var xNode in xNodes)
            {
                var xElem = (XmlElement)xNode;
                xElem.SetAttribute("port", model.Port.ToString());
            }

            xDoc.Save(appPath);//保存xml文档
        }

        /// <summary>
        /// 删除gen-csharp文件夹
        /// </summary>
        public static void ResetGenResultFolder(string shortShortFileName)
        {
            var resultDic = GenResultDic(shortShortFileName);
            Utility.DelDir(resultDic);
            Thread.Sleep(20);
            Utility.MakeDir(resultDic);
        }


        /// <summary>
        /// 通过注册表获取MSBuild的路径
        /// 如果失败,则默认组装X86 .net framework 4.0的地址
        /// </summary>
        public static string MsBuildPath()
        {
            var result = string.Empty;
            // 注册表读取
            RegistryKey rkey = null;
            try
            {
                rkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full");
                if (rkey != null)
                {
                    result = Path.Combine(rkey.GetValue("InstallPath").ToString(), "MSbuild.exe");
                }
            }
            finally
            {
                if (rkey != null)
                    rkey.Close();
            }

            if (File.Exists(result))
                return result;

            // 如果失败 直接拼接路径
            result = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                @"\Microsoft.NET\Framework\v4.0.30319\MSbuild.exe");

            if (File.Exists(result))
                return result;
            else
                return string.Empty;
        }
        #endregion
    }
}