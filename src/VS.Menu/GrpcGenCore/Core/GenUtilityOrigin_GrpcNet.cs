using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using VS.Menu.Helper;
using System.Linq;
using System.Collections.Generic;
using VS.Menu.Model;
using System.Xml;

namespace VS.Menu.GrpcGenCore
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class GenUtilityOrigin_GrpcNet
    {
        const string netcoreFold = "netcoreapp3.1";
        const string dllconfigFold = "dllconfigs";
        static string namespaces => DependenceHelper.GetGrpcNamespace();

        /// <summary>
        /// 从thrift命令生成的源文件dll中获取反射
        /// </summary>
        /// <param name="thriftDllPath"></param>
        /// <param name="dllconfigs">key:fileName,value:content</param>
        /// <returns></returns>
        public static string GetNamespace(string grpcDllPath)
        {
            var a2 = Assembly.LoadFrom(grpcDllPath);
            return a2.GetTypes().ToList().First().Namespace;
        }

        /// <summary>
        /// 生成项目的信息类
        /// </summary>
        /// <param name="thriftDllPath"></param>
        /// <returns></returns>
        public static string GenAssemblyInfoCs(string grpcDllPath)
        {
            var csNamespace = GetNamespace(grpcDllPath);
            var assemblyInfoBuilder = new StringBuilder();
            assemblyInfoBuilder.Append("using System.Reflection;");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("using System.Runtime.CompilerServices;");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("using System.Runtime.InteropServices;");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("[assembly: AssemblyTitle(\"" + csNamespace + "\")]");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("[assembly: AssemblyCompany(\"Overt\")]");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("[assembly: AssemblyProduct(\"" + csNamespace + "\")]");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("[assembly: AssemblyCopyright(\"Copyright © Overt " + DateTime.Now.Year + "\")]");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("[assembly: ComVisible(false)]");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("[assembly: AssemblyVersion(\"1.0.0.0\")]");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("[assembly: AssemblyFileVersion(\"1.0.0.0\")]");
            assemblyInfoBuilder.Append(Environment.NewLine);

            return assemblyInfoBuilder.ToString();
        }

        /// <summary>
        /// 移动到目录文件
        /// </summary>
        /// <param name="grpcDllPath"></param>
        public static string ToResultDic(string shortFileName, string grpcDllPath, string nameSpace, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                #region 移动Dll
                var resultDic = GrpcGlobal.GenResultDic(shortFileName);
                #endregion

                #region 移动Xml
                #endregion

                #region 移动Config
                var genFold = GrpcGlobal.GenFolder;

                var fmConfigFold = Path.Combine(genFold, "fmconfigs");
                var fmConfigs = Directory.GetFiles(fmConfigFold);


                

                //var newNet47ConfigFold = Path.Combine(resultDic, net47Fold, dllconfigFold);
                //Utility.MakeDic(newNet47ConfigFold);

                var coreConfigFold = Path.Combine(genFold, "coreconfigs");
                var coreConfigs = Directory.GetFiles(coreConfigFold);

                var newCoreConfigFold = Path.Combine(resultDic, netcoreFold, dllconfigFold);
                Utility.MakeDir(newCoreConfigFold);

                foreach (var coreConfig in coreConfigs)
                {
                    var file = new FileInfo(coreConfig);
                    File.Copy(coreConfig, Path.Combine(newCoreConfigFold, file.Name));
                }
                #endregion

                return resultDic;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// 生成Nuspec文件
        /// </summary>
        /// <param name="complieFold">编译后的数据的文件夹</param>
        /// <returns></returns>
        public static string GenNuspecXml(string complieFold, ServiceModel serviceModel, string csNamespace)
        {
            //var net47File = Directory.GetFiles(Path.Combine(complieFold, net47Fold));
            //var net47Configs = Directory.GetFiles(Path.Combine(complieFold, net47Fold, dllconfigFold));

            var netcore20File = Directory.GetFiles(Path.Combine(complieFold, netcoreFold));
            var netcore20Configs = Directory.GetFiles(Path.Combine(complieFold, netcoreFold, dllconfigFold));

            var nugetId = csNamespace;

            // 输出zookeeper.config
            var grpcBuilder = new StringBuilder();
            grpcBuilder.Append("<?xml version=\"1.0\"?>");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("<package>");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("  <metadata>");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("    <id>" + nugetId + "</id>");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("    <title>" + csNamespace + "</title>");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("    <version>1.0." + DateTime.Now.ToString("yyyyMMddHH") + "</version>");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("    <authors>Overt</authors>");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("    <owners>Overt</owners>");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("    <requireLicenseAcceptance>false</requireLicenseAcceptance>");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("    <description>" + csNamespace + "</description>");
            grpcBuilder.Append(Environment.NewLine);

            grpcBuilder.Append("    <copyright>Copyright " + DateTime.Now.Year + "</copyright>");
            grpcBuilder.Append(Environment.NewLine);

            // 依赖
            grpcBuilder.Append("    <dependencies>");
            grpcBuilder.Append(Environment.NewLine);

            var type = $"grpc";
            var dependencies = DependenceHelper.Get(type);
            foreach (var dependence in dependencies)
            {
                grpcBuilder.Append($"      <dependency id=\"{dependence.PackageId}\" version=\"{dependence.Version}\" />");
                grpcBuilder.Append(Environment.NewLine);
            }

            grpcBuilder.Append("    </dependencies>");
            grpcBuilder.Append(Environment.NewLine);

            grpcBuilder.Append("  </metadata>");
            grpcBuilder.Append(Environment.NewLine);

            // Dll文件
            grpcBuilder.Append("  <files>");
            grpcBuilder.Append(Environment.NewLine);
            
            //grpcBuilder.Append(Environment.NewLine);
            //foreach (var filePath in net47File)
            //{
            //    var file = new FileInfo(filePath);
            //    grpcBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/net47/" + file.Name + "\" />");
            //}
            grpcBuilder.Append(Environment.NewLine);
            foreach (var filePath in netcore20File)
            {
                var file = new FileInfo(filePath);
                grpcBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/netstandard2.0/" + file.Name + "\" />");
            }

            // configs
            
            //foreach (var config in net47Configs)
            //{
            //    var file = new FileInfo(config);
            //    var name = file.Name;
            //    if (file.Name.ToLower().IndexOf("client") > -1)
            //    {
            //        ChangeFmConfig(serviceModel, config);
            //        name = csNamespace + ".dll" + file.Extension;

            //        grpcBuilder.Append(Environment.NewLine);
            //        grpcBuilder.Append("    <file src=\"" + config + "\" target=\"/content/net47/dllconfigs/" + name + "\" />");
            //    }
            //}

            foreach (var config in netcore20Configs)
            {
                var file = new FileInfo(config);
                var name = file.Name;

                if (name.ToLower().IndexOf("client") > -1)
                {
                    ChangeCoreConfig(serviceModel, config);
                    name = csNamespace + ".dll" + file.Extension;

                    grpcBuilder.Append(Environment.NewLine);
                    grpcBuilder.Append("    <file src=\"" + config + "\" target=\"/content/netstandard2.0/dllconfigs/" + name + "\" />");
                }
            }

            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("  </files>");
            grpcBuilder.Append(Environment.NewLine);

            grpcBuilder.Append("</package>");
            grpcBuilder.Append(Environment.NewLine);

            return grpcBuilder.ToString();
        }


        /// <summary>
        /// 修改xml配置信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="configFile"></param>
        public static void ChangeFmConfig(ServiceModel model, string configFile)
        {
            var xDoc = new XmlDocument();
            xDoc.Load(configFile);//加载xml文件
            var serviceElem = (XmlElement)xDoc.SelectSingleNode("//service");
            if (serviceElem != null)
                serviceElem.SetAttribute("name", model.ServiceName);

            var xNode = serviceElem.SelectSingleNode("//endpoint");
            if (xNode != null)
                ((XmlElement)xNode).SetAttribute("port", model.Port.ToString());

            xDoc.Save(configFile);//保存xml文档
        }


        /// <summary>
        /// 修改Json配置信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="configFile"></param>
        public static void ChangeCoreConfig(ServiceModel model, string configFile)
        {
            var file = new FileInfo(configFile);
            var json = string.Empty;
            using (var fs = file.OpenText())
            {
                json = fs.ReadToEnd();
            }
            json = json.Replace("\"Name\": \"\"", "\"Name\": \"" + model.ServiceName + "\"");
            json = json.Replace("\"Port\": 0", "\"Port\": " + model.Port.ToString() + "");
            var bts = Encoding.UTF8.GetBytes(json);
            using (var fs = file.OpenWrite())
            {
                fs.Write(bts, 0, bts.Count());
            }
        }

    }
}