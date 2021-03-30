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
        const string netstandardFold = "netstandard2.1";
        const string netcoreappFold = "netcoreapp3.0";
        const string net5Fold = "net5.0";
        const string dllconfigFold = "dllconfigs";
        static string namespaces => DependenceHelper.GetGrpcNamespace("grpch2");

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
                Utility.MakeDir(Path.Combine(resultDic, netstandardFold));
                Utility.MakeDir(Path.Combine(resultDic, netcoreappFold));
                Utility.MakeDir(Path.Combine(resultDic, net5Fold));

                var netstandardFile = new FileInfo(grpcDllPath);
                var netcoreappFile = new FileInfo(grpcDllPath.Replace("netstandard2.1", netcoreappFold));
                var net5File = new FileInfo(grpcDllPath.Replace("netstandard2.1", net5Fold));

                File.Copy(netstandardFile.FullName, Path.Combine(resultDic, netstandardFold, nameSpace + ".dll"));
                File.Copy(netcoreappFile.FullName, Path.Combine(resultDic, netcoreappFold, nameSpace + ".dll"));
                File.Copy(net5File.FullName, Path.Combine(resultDic, net5Fold, nameSpace + ".dll"));
                #endregion

                #region 移动Xml
                var netstandardXmlFile = new FileInfo(grpcDllPath.Replace(".dll", ".xml")); 
                var netcoreappXmlFile = new FileInfo(grpcDllPath.Replace("netstandard2.1", netcoreappFold).Replace(".dll", ".xml"));
                var net5XmlFile = new FileInfo(grpcDllPath.Replace("netstandard2.1", net5Fold).Replace(".dll", ".xml"));

                File.Copy(netstandardXmlFile.FullName, Path.Combine(resultDic, netstandardFold, nameSpace + ".xml"));
                File.Copy(netcoreappXmlFile.FullName, Path.Combine(resultDic, netcoreappFold, nameSpace + ".xml"));
                File.Copy(net5XmlFile.FullName, Path.Combine(resultDic, net5Fold, nameSpace + ".xml"));
                #endregion

                #region 移动Config
                var genFold = GrpcGlobal.GenFolder;

                var coreConfigFold = Path.Combine(genFold, "coreconfigs");
                var coreConfigs = Directory.GetFiles(coreConfigFold);

                var newNetStandardFold = Path.Combine(resultDic, netstandardFold, dllconfigFold);
                var newNetCoreAppFold = Path.Combine(resultDic, netcoreappFold, dllconfigFold);
                var newNet5Fold = Path.Combine(resultDic, net5Fold, dllconfigFold);
                Utility.MakeDir(newNetStandardFold);
                Utility.MakeDir(newNetCoreAppFold);
                Utility.MakeDir(newNet5Fold);

                foreach (var coreConfig in coreConfigs)
                {
                    var file = new FileInfo(coreConfig);
                    File.Copy(coreConfig, Path.Combine(newNetStandardFold, file.Name));
                    File.Copy(coreConfig, Path.Combine(newNetCoreAppFold, file.Name));
                    File.Copy(coreConfig, Path.Combine(newNet5Fold, file.Name));
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
            var netstandardFile = Directory.GetFiles(Path.Combine(complieFold, netstandardFold));
            var netstandardConfigs = Directory.GetFiles(Path.Combine(complieFold, netstandardFold, dllconfigFold));

            var netcoreappFile = Directory.GetFiles(Path.Combine(complieFold, netcoreappFold));
            var netcoreappConfigs = Directory.GetFiles(Path.Combine(complieFold, netcoreappFold, dllconfigFold));

            var net5File = Directory.GetFiles(Path.Combine(complieFold, net5Fold));
            var net5Configs = Directory.GetFiles(Path.Combine(complieFold, net5Fold, dllconfigFold));

            var nugetId = csNamespace;

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

            var type = $"grpch2";
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
            
            grpcBuilder.Append(Environment.NewLine);
            foreach (var filePath in netstandardFile)
            {
                var file = new FileInfo(filePath);
                grpcBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/netstandard2.1/" + file.Name + "\" />");
            }
            foreach (var filePath in netcoreappFile)
            {
                var file = new FileInfo(filePath);
                grpcBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/netcoreapp3.0/" + file.Name + "\" />");
            }
            foreach (var filePath in net5File)
            {
                var file = new FileInfo(filePath);
                grpcBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/net5.0/" + file.Name + "\" />");
            }

            // configs
            foreach (var config in netstandardConfigs)
            {
                var file = new FileInfo(config);
                var name = file.Name;

                if (name.ToLower().IndexOf("client") > -1)
                {
                    ChangeCoreConfig(serviceModel, config);
                    name = csNamespace + ".dll" + file.Extension;

                    grpcBuilder.Append(Environment.NewLine);
                    grpcBuilder.Append("    <file src=\"" + config + "\" target=\"/content/netstandard2.1/dllconfigs/" + name + "\" />");
                }
            }
            foreach (var config in netcoreappConfigs)
            {
                var file = new FileInfo(config);
                var name = file.Name;

                if (name.ToLower().IndexOf("client") > -1)
                {
                    ChangeCoreConfig(serviceModel, config);
                    name = csNamespace + ".dll" + file.Extension;

                    grpcBuilder.Append(Environment.NewLine);
                    grpcBuilder.Append("    <file src=\"" + config + "\" target=\"/content/netcoreapp3.0/dllconfigs/" + name + "\" />");
                }
            }
            foreach (var config in net5Configs)
            {
                var file = new FileInfo(config);
                var name = file.Name;

                if (name.ToLower().IndexOf("client") > -1)
                {
                    ChangeCoreConfig(serviceModel, config);
                    name = csNamespace + ".dll" + file.Extension;

                    grpcBuilder.Append(Environment.NewLine);
                    grpcBuilder.Append("    <file src=\"" + config + "\" target=\"/content/net5.0/dllconfigs/" + name + "\" />");
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