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
    public class GenUtilityOrigin
    {
        const string net45Fold = "net45";
        const string net46Fold = "net46";
        //const string net47Fold = "net47";
        const string netcoreFold = "netstandard2.0";
        const string dllconfigFold = "dllconfigs";
        static string namespaces => DependenceHelper.GetGrpcNamespace();
        static string depType = $"grpc";
        static List<DependenceModel> dependencies => DependenceHelper.Get(depType);
        static DependenceModel overtCoreGrpcPag => dependencies.FirstOrDefault(oo => oo.PackageId.ToLower() == namespaces.ToLower());

        /// <summary>
        /// 执行thrift -gen csharp .thrift 命令得到生成的.cs文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="output"></param>
        /// <param name="error"></param>
        /// <returns>
        /// 是否成功以最终是否生成gen-csharp文件为准 这里会out出输出和错误 
        /// </returns>
        public static FileInfo[] GenGrpc(string filePath, out string error, out string csNamespace)
        {
            error = string.Empty;
            csNamespace = string.Empty;
            Utility.ClearDir(GrpcGlobal.GenFolder);

            #region 获取Import文件
            var fileInfo = new FileInfo(filePath);
            var sr = fileInfo.OpenText();
            var nextLine = string.Empty;
            var protos = new List<string>();
            while ((nextLine = sr.ReadLine()) != null)
            {
                if (nextLine.IndexOf("import") > -1)
                {
                    var proto = nextLine.Replace("import", "").Replace("\"", "").Replace(";", "").Trim();
                    if (proto.EndsWith(".proto"))
                        protos.Add(proto);
                }
                
                if(nextLine.IndexOf("package") > -1)
                    csNamespace = nextLine.Replace("package", "").Replace("\"", "").Replace(";", "").Trim();
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

            var protocPath = GrpcGlobal.ProtocExePath_1_9_0;

            if (IsGte1_0_5())
                protocPath = GrpcGlobal.ProtocExePath_New_2_36_4;

            Utility.InvokeProcess(protocPath, null, out output, out error, arguments: cmd);

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
            if ((classes?.Count() ?? 0) <= 0)
                throw new Exception("不存在任何Service");

            var clientClass = classes.First();

            csNamespace = clientClass.Namespace;
            var clientClassFullName = clientClass.FullName;
            var mainClassName = clientClassFullName.Split('+')[0];
            var clientName = clientClass.Name;

            // 依赖的命名空间可能会改
            var grpcBuilder = new StringBuilder();
            grpcBuilder.Append($"using Grpc.Core;");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append($"using Grpc.Core.Interceptors;");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append($"using {namespaces};");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append($"using {namespaces}.Intercept;");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("using System;");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("using System.Collections.Concurrent;");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("using System.Collections.Generic;");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("using System.IO;");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("using __GrpcService = " + mainClassName + ";");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("namespace " + csNamespace);
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("{");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("#if NET45 || NET46 || NET47");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("public class ClientManager {");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("public static IClientTracer Tracer { get; set; } = default(IClientTracer);");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("public static List<Interceptor> Interceptors { get; } = new List<Interceptor>();");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("private static string DefaultConfigPath { get; set; } = \"dllconfigs/" + csNamespace + ".dll.config\";");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("public static __GrpcService." + clientName + " Instance{get{");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("return ClientManager<__GrpcService." + clientName + ">.Instance;");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("} }");
            grpcBuilder.Append(Environment.NewLine);
            if (IsGte1_0_5())
            {
                grpcBuilder.Append("public static __GrpcService." + clientName + " CreateInstance(Func<List<ServerCallInvoker>, ServerCallInvoker> getInvoker = null){");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("return ClientManager<__GrpcService.GrpcExampleServiceClient>.CreateInstance(getInvoker);");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("}");
                grpcBuilder.Append(Environment.NewLine);
            }

            // 配置
            grpcBuilder.Append("private static readonly ConcurrentDictionary<Type, string> configMap = new ConcurrentDictionary<Type, string>();");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("public static void Configure<T>(string configPath) { configMap.AddOrUpdate(typeof(T), configPath, (t, s) => configPath); }");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("public static string GetConfigure<T>() { if (configMap.TryGetValue(typeof(T), out string configPath)) return configPath; return DefaultConfigPath;}");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("}");
            grpcBuilder.Append(Environment.NewLine);

            // 多服务
            grpcBuilder.Append("public class ClientManager<T> : ClientManager where T : ClientBase");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("{");
            grpcBuilder.Append(Environment.NewLine);
            if (IsGte1_0_5())
            {
                grpcBuilder.Append("public static new T Instance => CreateInstance();");
                grpcBuilder.Append(Environment.NewLine);

                grpcBuilder.Append("public static new T CreateInstance(Func<List<ServerCallInvoker>, ServerCallInvoker> getInvoker = null) {");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("var configPath = GetConfigure<T>();");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("var options = new GrpcClientOptions() { Tracer = Tracer };");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("if (Interceptors?.Count > 0) options.Interceptors.AddRange(Interceptors);");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("return GrpcClientManager<T>.Get(configPath, options, getInvoker);");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("}");
                grpcBuilder.Append(Environment.NewLine);
            }
            else
            {
                grpcBuilder.Append("public static new T Instance{ get {");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("var configPath = GetConfigure<T>();");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("var abConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configPath);");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("return GrpcClientManager<T>.Get(abConfigPath, Tracer);");
                grpcBuilder.Append(Environment.NewLine);
                grpcBuilder.Append("} }");
                grpcBuilder.Append(Environment.NewLine);
            }


            grpcBuilder.Append("}");

            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("#endif");
            grpcBuilder.Append(Environment.NewLine);
            grpcBuilder.Append("}");

            return grpcBuilder.ToString();
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
        /// 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> MakeCoreConfig_GrpcNet()
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
            clientConfig.Append("      \"Scheme\":  \"http\",");
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
            foreach (var filePath in net45File)
            {
                var file = new FileInfo(filePath);
                grpcBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/net45/" + file.Name + "\" />");
            }
            grpcBuilder.Append(Environment.NewLine);
            foreach (var filePath in net46File)
            {
                var file = new FileInfo(filePath);
                grpcBuilder.Append("    <file src=\"" + filePath + "\" target=\"/lib/net46/" + file.Name + "\" />");
            }
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
            foreach (var config in net45Configs)
            {
                var file = new FileInfo(config);
                var name = file.Name;
                if (file.Name.ToLower().IndexOf("client") > -1)
                {
                    ChangeFmConfig(serviceModel, config);
                    name = csNamespace + ".dll" + file.Extension;

                    grpcBuilder.Append(Environment.NewLine);
                    grpcBuilder.Append("    <file src=\"" + config + "\" target=\"/content/net45/dllconfigs/" + name + "\" />");
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

                    grpcBuilder.Append(Environment.NewLine);
                    grpcBuilder.Append("    <file src=\"" + config + "\" target=\"/content/net46/dllconfigs/" + name + "\" />");
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

        public static bool IsGte1_0_5()
        {
            var gte1_0_5 = false;
            try
            {
                gte1_0_5 = new Version(overtCoreGrpcPag?.Version?.Split('-')[0]) > new Version("1.0.4.1");
            }
            catch (Exception ex)
            {

            }
            return gte1_0_5;
        }

    }
}