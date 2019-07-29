﻿using System;
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
    public class GenUtilityOrigin
    {

        const string net45Fold = "net45";
        const string net46Fold = "net46";
        //const string net47Fold = "net47";
        const string netcoreFold = "netstandard2.0";
        const string dllconfigFold = "dllconfigs";
        static string namespaces => DependenceHelper.GetGrpcNamespace();

        /// <summary>
        /// 执行thrift -gen csharp .thrift 命令得到生成的.cs文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="output"></param>
        /// <param name="error"></param>
        /// <returns>
        /// 是否成功以最终是否生成gen-csharp文件为准 这里会out出输出和错误 
        /// </returns>
        public static FileInfo[] GenGrpc(string filePath, out string error)
        {
            error = string.Empty;
            Utility.ClearDir(GrpcGlobal.GenFolder);

            #region 获取Import文件
            var fileInfo = new FileInfo(filePath);
            var sr = fileInfo.OpenText();
            var nextLine = string.Empty;
            var protos = new List<string>();
            while ((nextLine = sr.ReadLine()) != null)
            {
                if (nextLine.IndexOf("import") == -1)
                    continue;
                var proto = nextLine.Replace("import", "").Replace("\"", "").Replace(";", "").Trim();
                if (!proto.EndsWith(".proto"))
                    continue;

                protos.Add(proto);
            }
            #endregion

            var fileDic = new FileInfo(filePath).Directory.FullName;
            var cmdArr = new string[]
            {
                "-I\""+fileDic+"\"",
                "--csharp_out",
                "\""+GrpcGlobal.GenFolder+"\"",
                "\""+filePath+"\"",
                "--grpc_out",
                "\""+GrpcGlobal.GenFolder+"\"",
                (protos.Count>0 ? "--descriptor_set_out=\""+filePath+"bin\"":""),
                (protos.Count>0 ? "--include_imports \"" + string.Join("\" \"", protos) + "\"":""),
                "--plugin=protoc-gen-grpc="+GrpcGlobal.CsharpPluginsPath
            };
            var cmd = string.Join(" ", cmdArr);
            var output = string.Empty;
            Utility.InvokeProcess(GrpcGlobal.ProtoExePath, null, out output, out error, arguments: cmd);

            var basePath = GrpcGlobal.GenFolder;
            if (!Directory.Exists(basePath))
                return null;
            var files = new DirectoryInfo(basePath).GetFiles();
            if (files.Length <= 0)
                return null;

            error = string.Empty;
            return files;
        }

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
        /// 生成异步代理类
        /// </summary>
        public static string GenAsyncProxyCs(string grpcDllPath, out string csNamespace)
        {
            var a2 = Assembly.LoadFrom(grpcDllPath);
            var classes = a2.GetExportedTypes().Where(oo => oo.FullName.IndexOf("+") > -1 && oo.FullName.EndsWith("Client"));
            var clientClass = classes.First();

            csNamespace = clientClass.Namespace;
            var clientClassFullName = clientClass.FullName;
            var mainClassName = clientClassFullName.Split('+')[0];
            var clientName = clientClass.Name;

            // 依赖的命名空间可能会改
            var namespaces = DependenceHelper.GetGrpcNamespace();
            var thriftProxyBuilder = new StringBuilder();
            thriftProxyBuilder.Append($"using {namespaces};");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append($"using {namespaces}.Intercept;");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("using System;");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("using System.IO;");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("using __GrpcService = " + mainClassName + ";");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("namespace " + csNamespace);
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("{");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("#if NET45 || NET46 || NET47");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("public class ClientManager {");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("public static IClientTracer Tracer { get; set; } = default(IClientTracer);");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("public static __GrpcService." + clientName + " Instance{get{");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, \"dllconfigs/" + csNamespace + ".dll.config\");");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("return GrpcClientManager<__GrpcService." + clientName + ">.Get(configPath, Tracer);");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("} }");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("}");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("#endif");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("}");

            return thriftProxyBuilder.ToString();
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
            assemblyInfoBuilder.Append("[assembly: AssemblyCompany(\"Sodao\")]");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("[assembly: AssemblyProduct(\"" + csNamespace + "\")]");
            assemblyInfoBuilder.Append(Environment.NewLine);
            assemblyInfoBuilder.Append("[assembly: AssemblyCopyright(\"Copyright © Sodao " + DateTime.Now.Year + "\")]");
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

                Utility.MakeDir(Path.Combine(resultDic, net45Fold));
                Utility.MakeDir(Path.Combine(resultDic, net46Fold));
                //Utility.MakeDic(Path.Combine(resultDic, net47Fold));
                Utility.MakeDir(Path.Combine(resultDic, netcoreFold));

                var net45File = new FileInfo(grpcDllPath.Replace("net46", net45Fold));
                var net46File = new FileInfo(grpcDllPath);
                //var net47File = new FileInfo(grpcDllPath.Replace("net46", "net47"));
                var netcore20 = new FileInfo(grpcDllPath.Replace("net46", netcoreFold));

                File.Copy(net45File.FullName, Path.Combine(resultDic, net45Fold, nameSpace + ".dll"));
                File.Copy(net46File.FullName, Path.Combine(resultDic, net46Fold, nameSpace + ".dll"));
                //File.Copy(net47File.FullName, Path.Combine(resultDic, net47Fold, nameSpace + ".dll"));
                File.Copy(netcore20.FullName, Path.Combine(resultDic, netcoreFold, nameSpace + ".dll"));
                #endregion


                #region 移动Xml
                var net45XmlFile = new FileInfo(grpcDllPath.Replace("net46", net45Fold).Replace(".dll", ".xml"));
                var net46XmlFile = new FileInfo(grpcDllPath.Replace(".dll", ".xml"));
                //var net47XmlFile = new FileInfo(grpcDllPath.Replace("net46", "net47").Replace(".dll", ".xml"));
                var netcore20Xml = new FileInfo(grpcDllPath.Replace("net46", netcoreFold).Replace(".dll", ".xml"));

                File.Copy(net45XmlFile.FullName, Path.Combine(resultDic, net45Fold, nameSpace + ".xml"));
                File.Copy(net46XmlFile.FullName, Path.Combine(resultDic, net46Fold, nameSpace + ".xml"));
                //File.Copy(net47XmlFile.FullName, Path.Combine(resultDic, net47Fold, nameSpace + ".dll"));
                File.Copy(netcore20Xml.FullName, Path.Combine(resultDic, netcoreFold, nameSpace + ".xml"));
                #endregion

                #region 移动Config
                var genFold = GrpcGlobal.GenFolder;

                var fmConfigFold = Path.Combine(genFold, "fmconfigs");
                var fmConfigs = Directory.GetFiles(fmConfigFold);


                var newNet45ConfigFold = Path.Combine(resultDic, net45Fold, dllconfigFold);
                Utility.MakeDir(newNet45ConfigFold);

                var newNet46ConfigFold = Path.Combine(resultDic, net46Fold, dllconfigFold);
                Utility.MakeDir(newNet46ConfigFold);

                //var newNet47ConfigFold = Path.Combine(resultDic, net47Fold, dllconfigFold);
                //Utility.MakeDic(newNet47ConfigFold);

                foreach (var fmConfig in fmConfigs)
                {
                    var file = new FileInfo(fmConfig);
                    File.Copy(fmConfig, Path.Combine(newNet45ConfigFold, file.Name));
                    File.Copy(fmConfig, Path.Combine(newNet46ConfigFold, file.Name));
                    //File.Copy(fmConfig, Path.Combine(newNet47ConfigFold, file.Name));
                }

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
        /// 制作配置文件
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> MakeFmConfig()
        {
            var result = new Dictionary<string, string>();
            // 输出Consul.config
            var consulConfig = new StringBuilder();
            consulConfig.Append("<?xml version=\"1.0\"?>");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("<configuration>");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("     <configSections>");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append($"          <section name=\"consulServer\" type=\"{namespaces}.ConsulServerSection, {namespaces}\"/>");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("     </configSections>");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("     <consulServer>");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("        <service address=\"http://127.0.0.1:8500\" />");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("    </consulServer>");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("</configuration>");
            result.Add("Consul.config", consulConfig.ToString());

            // 输出Client.config
            var clientConfig = new StringBuilder();
            clientConfig.Append("<?xml version=\"1.0\"?>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("<configuration>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("     <configSections>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append($"          <section name=\"grpcClient\" type=\"{namespaces}.GrpcClientSection, {namespaces}\"/>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("     </configSections>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("     <grpcClient>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("         <service name=\"\" maxRetry=\"0\">");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("             <discovery>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("                 <server>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("                     <endpoint host=\"127.0.0.1\" port=\"\" />");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("                 </server>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("                 <consul path=\"dllconfigs/Consul.config\" />");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("             </discovery>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("         </service>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("    </grpcClient>");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("</configuration>");
            result.Add("Client.config", clientConfig.ToString());

            // 输出Consul.config
            var serverConfig = new StringBuilder();
            serverConfig.Append("<?xml version=\"1.0\"?>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("<configuration>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("     <configSections>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append($"          <section name=\"grpcServer\" type=\"{namespaces}.GrpcServerSection, {namespaces}\"/>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("     </configSections>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("     <grpcServer>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("         <service name=\"\" port=\"\">");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("             <registry>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("                 <consul path=\"dllconfigs/Consul.config\" />");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("             </registry>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("         </service>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("    </grpcServer>");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("</configuration>");
            result.Add("Server.config", serverConfig.ToString());

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> MakeCoreConfig()
        {
            var result = new Dictionary<string, string>();
            // 输出Consul.config
            var consulConfig = new StringBuilder();
            consulConfig.Append("{");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("  \"ConsulServer\": {");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("    \"Service\": {");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("      \"Address\": \"http://127.0.0.1:8500\"");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("    }");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("  }");
            consulConfig.Append(Environment.NewLine);
            consulConfig.Append("}");
            result.Add("consulsettings.json", consulConfig.ToString());

            // 输出Client.config
            var clientConfig = new StringBuilder();
            clientConfig.Append("{");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("  \"GrpcClient\": {");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("    \"Service\": {");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("      \"Name\": \"\",");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("      \"MaxRetry\":  0,");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("      \"Discovery\": {");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("        \"EndPoints\": [");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("          {");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("            \"Host\": \"127.0.0.1\",");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("            \"Port\": 0");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("          }");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("        ],");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("        \"Consul\": {");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("          \"Path\": \"dllconfigs/consulsettings.json\"");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("        }");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("      }");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("    }");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("  }");
            clientConfig.Append(Environment.NewLine);
            clientConfig.Append("}");
            result.Add("clientsettings.json", clientConfig.ToString());

            // 输出Consul.config
            var serverConfig = new StringBuilder();
            serverConfig.Append("{");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("  \"GrpcServer\": {");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("    \"Service\": {");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("      \"Name\": \"grpcservice\",");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("      \"Host\": \"\",");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("      \"Port\": 10001,");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("      \"Consul\": {");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("        \"Path\": \"dllconfigs/consulsettings.json\"");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("      }");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("    }");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("  }");
            serverConfig.Append(Environment.NewLine);
            serverConfig.Append("}");
            result.Add("grpcsettings.json", serverConfig.ToString());

            return result;
        }

        /// <summary>
        /// 生成Nuspec文件
        /// </summary>
        /// <param name="complieFold">编译后的数据的文件夹</param>
        /// <returns></returns>
        public static string GenNuspecXml(string complieFold, ServiceModel serviceModel, string csNamespace)
        {
            var net45File = Directory.GetFiles(Path.Combine(complieFold, net45Fold));
            var net45Configs = Directory.GetFiles(Path.Combine(complieFold, net45Fold, dllconfigFold));

            var net46File = Directory.GetFiles(Path.Combine(complieFold, net46Fold));
            var net46Configs = Directory.GetFiles(Path.Combine(complieFold, net46Fold, dllconfigFold));

            //var net47File = Directory.GetFiles(Path.Combine(complieFold, net47Fold));
            //var net47Configs = Directory.GetFiles(Path.Combine(complieFold, net47Fold, dllconfigFold));

            var netcore20File = Directory.GetFiles(Path.Combine(complieFold, netcoreFold));
            var netcore20Configs = Directory.GetFiles(Path.Combine(complieFold, netcoreFold, dllconfigFold));

            var nugetId = csNamespace;

            // 输出zookeeper.config
            var thriftProxyBuilder = new StringBuilder();
            thriftProxyBuilder.Append("<?xml version=\"1.0\"?>");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("<package>");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("  <metadata>");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("    <id>" + nugetId + "</id>");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("    <title>" + csNamespace + "</title>");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("    <version>1.0." + DateTime.Now.ToString("yyyyMMddHH") + "</version>");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("    <authors>Sodao</authors>");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("    <owners>Sodao</owners>");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("    <requireLicenseAcceptance>false</requireLicenseAcceptance>");
            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("    <description>" + csNamespace + "</description>");
            thriftProxyBuilder.Append(Environment.NewLine);

            thriftProxyBuilder.Append("    <copyright>Copyright " + DateTime.Now.Year + "</copyright>");
            thriftProxyBuilder.Append(Environment.NewLine);

            // 依赖
            thriftProxyBuilder.Append("    <dependencies>");
            thriftProxyBuilder.Append(Environment.NewLine);

            var type = $"grpc";
            var dependencies = DependenceHelper.Get(type);
            foreach (var dependence in dependencies)
            {
                thriftProxyBuilder.Append($"      <dependency id=\"{dependence.PackageId}\" version=\"{dependence.Version}\" />");
                thriftProxyBuilder.Append(Environment.NewLine);
            }

            thriftProxyBuilder.Append("    </dependencies>");
            thriftProxyBuilder.Append(Environment.NewLine);

            thriftProxyBuilder.Append("  </metadata>");
            thriftProxyBuilder.Append(Environment.NewLine);

            // Dll文件
            thriftProxyBuilder.Append("  <files>");
            thriftProxyBuilder.Append(Environment.NewLine);
            foreach (var filePath in net45File)
            {
                var file = new FileInfo(filePath);
                thriftProxyBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/net45/" + file.Name + "\" />");
            }
            thriftProxyBuilder.Append(Environment.NewLine);
            foreach (var filePath in net46File)
            {
                var file = new FileInfo(filePath);
                thriftProxyBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/net46/" + file.Name + "\" />");
            }
            //thriftProxyBuilder.Append(Environment.NewLine);
            //foreach (var filePath in net47File)
            //{
            //    var file = new FileInfo(filePath);
            //    thriftProxyBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/net47/" + file.Name + "\" />");
            //}
            thriftProxyBuilder.Append(Environment.NewLine);
            foreach (var filePath in netcore20File)
            {
                var file = new FileInfo(filePath);
                thriftProxyBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/netstandard2.0/" + file.Name + "\" />");
            }

            // configs
            foreach (var config in net45Configs)
            {
                var file = new FileInfo(config);
                var name = file.Name;
                if (file.Name.ToLower().IndexOf("client") > -1)
                {
                    ChangeFmConfig(serviceModel, config);
                    name = csNamespace + ".dll" + file.Extension;

                    thriftProxyBuilder.Append(Environment.NewLine);
                    thriftProxyBuilder.Append("    <file src=\"" + config + "\" target=\"/content/net45/dllconfigs/" + name + "\" />");
                }
            }
            foreach (var config in net46Configs)
            {
                var file = new FileInfo(config);
                var name = file.Name;
                if (file.Name.ToLower().IndexOf("client") > -1)
                {
                    ChangeFmConfig(serviceModel, config);
                    name = csNamespace + ".dll" + file.Extension;

                    thriftProxyBuilder.Append(Environment.NewLine);
                    thriftProxyBuilder.Append("    <file src=\"" + config + "\" target=\"/content/net46/dllconfigs/" + name + "\" />");
                }
            }
            //foreach (var config in net47Configs)
            //{
            //    var file = new FileInfo(config);
            //    var name = file.Name;
            //    if (file.Name.ToLower().IndexOf("client") > -1)
            //    {
            //        ChangeFmConfig(serviceModel, config);
            //        name = csNamespace + ".dll" + file.Extension;

            //        thriftProxyBuilder.Append(Environment.NewLine);
            //        thriftProxyBuilder.Append("    <file src=\"" + config + "\" target=\"/content/net47/dllconfigs/" + name + "\" />");
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

                    thriftProxyBuilder.Append(Environment.NewLine);
                    thriftProxyBuilder.Append("    <file src=\"" + config + "\" target=\"/content/netstandard2.0/dllconfigs/" + name + "\" />");
                }
            }

            thriftProxyBuilder.Append(Environment.NewLine);
            thriftProxyBuilder.Append("  </files>");
            thriftProxyBuilder.Append(Environment.NewLine);

            thriftProxyBuilder.Append("</package>");
            thriftProxyBuilder.Append(Environment.NewLine);

            return thriftProxyBuilder.ToString();
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