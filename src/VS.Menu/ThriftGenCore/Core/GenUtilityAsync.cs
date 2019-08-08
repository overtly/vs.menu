using VS.Menu.ThriftGenCore.AsyncGen;
using VS.Menu.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using VS.Menu.Helper;

namespace VS.Menu.ThriftGenCore
{
    /// <summary>
    /// 生成异步代理类的工具
    /// </summary>
    public class GenUtilityAsync
    {
        /// <summary>
        /// 从thrift命令生成的源文件dll中获取反射
        /// </summary>
        /// <param name="thriftDllPath"></param>
        /// <param name="dllconfigs">key:fileName,value:content</param>
        /// <returns></returns>
        public static List<CodeResult> GenAsync(string thriftDllPath, string thriftDllXmlPath, out Dictionary<string, string> dllconfigs)
        {
            // 加载依赖
            LoadDependencies();

            // 尝试把文件加载到字节流中读取
            byte[] b = File.ReadAllBytes(thriftDllPath);
            Assembly a2 = Assembly.Load(b);
            // 获取其中公开的接口类型
            var interfaces = a2.GetExportedTypes().Where(c => c.IsInterface).ToArray();
            // 返回反射出来的dllconfig
            dllconfigs = MakeDllconfig(interfaces);

            //读取xml
            var dllXml = new XmlDocument();
            if (File.Exists(thriftDllXmlPath))
                dllXml.Load(thriftDllXmlPath);

            // Build
            return Build("4.0", interfaces, dllXml);
        }

        /// <summary>
        /// 从thrift命令生成的源文件dll中获取反射
        /// </summary>
        /// <param name="thriftDllPath"></param>
        /// <param name="dllconfigs">key:fileName,value:content</param>
        /// <returns></returns>
        public static string GetNamespace(string thriftDllPath)
        {
            // 加载依赖
            LoadDependencies();

            // 尝试把文件加载到字节流中读取
            byte[] b = File.ReadAllBytes(thriftDllPath);
            Assembly a2 = Assembly.Load(b);
            return a2.GetTypes().First().Namespace;
        }

        /// <summary>
        /// 生成异步代理类
        /// </summary>
        public static string GenAsyncProxyCs(string thriftDllPath, ServiceModel serviceModel, out string csNamespace)
        {
            csNamespace = string.Empty;
            var serviceName = serviceModel.ServiceName;
            var configServiceName = serviceModel.ConfigServiceName;
            var thriftProxyBuilder = new StringBuilder();

            // 加载依赖
            LoadDependencies();

            // 尝试把文件加载到字节流中读取
            byte[] b = File.ReadAllBytes(thriftDllPath);
            Assembly a2 = Assembly.Load(b);
            // 获取其中公开的接口类型
            var interfaces = a2.GetExportedTypes().Where(c => c.IsInterface).ToArray();
            if (interfaces.Length > 0)
            {
                csNamespace = interfaces.First().Namespace;

                var isMultiInterface = interfaces.Length > 1;
                //string declaringName = string.Empty;
                //if (interfaces.First().DeclaringType != null)
                //    declaringName = interfaces.First().DeclaringType.Name;

                thriftProxyBuilder.Append("using Thrift.Client;");
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("namespace " + csNamespace);
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("{");
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("static public class ThriftProxy {");

                for (int i = 0; i < interfaces.Length; i++)
                {
                    var declaringName = interfaces[i].DeclaringType.Name;
                    if (isMultiInterface)
                    {
                        serviceName = declaringName;
                        configServiceName = declaringName;
                    }

                    thriftProxyBuilder.Append(Environment.NewLine);
                    thriftProxyBuilder.Append(string.Format("static public readonly Async{0}.Iface_client {1} = ", declaringName, serviceName));
                    thriftProxyBuilder.Append(Environment.NewLine);
                    thriftProxyBuilder.Append(string.Format("ThriftClientManager.GetClient<Async{0}.Iface_client>(", declaringName));
                    thriftProxyBuilder.Append(Environment.NewLine);
                    thriftProxyBuilder.Append(string.Format("\"{0}\", @\"dllconfigs\\{1}.dll.config\");", configServiceName, csNamespace));
                    thriftProxyBuilder.Append(Environment.NewLine);
                }

                thriftProxyBuilder.Append("}");
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("}");
                thriftProxyBuilder.Append(Environment.NewLine);
            }
            return thriftProxyBuilder.ToString();
        }

        /// <summary>
        /// 生成项目的信息类
        /// </summary>
        /// <param name="thriftDllPath"></param>
        /// <returns></returns>
        public static string GenAssemblyInfoCs(string thriftDllPath, out string csNamespace)
        {
            csNamespace = string.Empty;

            // 加载依赖
            LoadDependencies();

            // 尝试把文件加载到字节流中读取
            byte[] b = File.ReadAllBytes(thriftDllPath);
            Assembly a2 = Assembly.Load(b);
            // 获取其中公开的接口类型
            var interfaces = a2.GetExportedTypes().Where(c => c.IsInterface).ToArray();
            if (interfaces.Length > 0)
            {
                csNamespace = interfaces.First().Namespace;
                var assemblyInfoBuilder = new StringBuilder();
                assemblyInfoBuilder.Append("using System.Reflection;");
                assemblyInfoBuilder.Append(Environment.NewLine);
                assemblyInfoBuilder.Append("using System.Runtime.CompilerServices;");
                assemblyInfoBuilder.Append(Environment.NewLine);
                assemblyInfoBuilder.Append("using System.Runtime.InteropServices;");
                assemblyInfoBuilder.Append(Environment.NewLine);
                assemblyInfoBuilder.Append("[assembly: AssemblyTitle(\"" + csNamespace + "\")]");
                assemblyInfoBuilder.Append(Environment.NewLine);
                assemblyInfoBuilder.Append("[assembly: AssemblyCompany(\"Microsoft\")]");
                assemblyInfoBuilder.Append(Environment.NewLine);
                assemblyInfoBuilder.Append("[assembly: AssemblyProduct(\"" + csNamespace + "\")]");
                assemblyInfoBuilder.Append(Environment.NewLine);
                assemblyInfoBuilder.Append("[assembly: AssemblyCopyright(\"Copyright © Microsoft " + DateTime.Now.Year + "\")]");
                assemblyInfoBuilder.Append(Environment.NewLine);
                assemblyInfoBuilder.Append("[assembly: ComVisible(false)]");
                //assemblyInfoBuilder.Append(Environment.NewLine);
                //assemblyInfoBuilder.Append("[assembly: Guid(\"73d22503-3cec-45c1-b289-58af790da525\")]");
                assemblyInfoBuilder.Append(Environment.NewLine);
                assemblyInfoBuilder.Append("[assembly: AssemblyVersion(\"1.0.0.0\")]");
                assemblyInfoBuilder.Append(Environment.NewLine);
                assemblyInfoBuilder.Append("[assembly: AssemblyFileVersion(\"1.0.0.0\")]");
                assemblyInfoBuilder.Append(Environment.NewLine);

                return assemblyInfoBuilder.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// 生成Nuspec文件
        /// </summary>
        /// <param name="thriftDllPath"></param>
        /// <returns></returns>
        public static string GenNuspecXml(string thriftDllPath, string nugetId = "")
        {
            // 加载依赖
            LoadDependencies();

            // 尝试把文件加载到字节流中读取
            byte[] b = File.ReadAllBytes(thriftDllPath);
            Assembly a2 = Assembly.Load(b);
            // 获取其中公开的接口类型
            var interfaces = a2.GetExportedTypes().Where(c => c.IsInterface).ToArray();
            if (interfaces.Length > 0)
            {
                var csNamespace = interfaces.First().Namespace;
                var declaringName = string.Empty;
                if (interfaces.First().DeclaringType != null)
                    declaringName = interfaces.First().DeclaringType.Name;

                if (string.IsNullOrEmpty(nugetId))
                    nugetId = csNamespace;

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
                thriftProxyBuilder.Append("    <authors>Overt</authors>");
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("    <owners>Overt</owners>");
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("    <requireLicenseAcceptance>false</requireLicenseAcceptance>");
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("    <description>" + csNamespace + "</description>");
                thriftProxyBuilder.Append(Environment.NewLine);


                thriftProxyBuilder.Append("    <copyright>Copyright " + DateTime.Now.Year + "</copyright>");
                thriftProxyBuilder.Append(Environment.NewLine);

                // 依赖
                var type = $"thrift_{ThriftGlobal.GenAsyncVersion}";
                var dependencies = DependenceHelper.Get(type);
                thriftProxyBuilder.Append("    <dependencies>");
                thriftProxyBuilder.Append(Environment.NewLine);
                foreach (var dependence in dependencies)
                {
                    thriftProxyBuilder.Append($"      <dependency id=\"{dependence.PackageId}\" version=\"{dependence.Version}\" />");
                    thriftProxyBuilder.Append(Environment.NewLine);
                }
                thriftProxyBuilder.Append("    </dependencies>");
                thriftProxyBuilder.Append(Environment.NewLine);

                //if (ThriftGlobal.GenAsyncVersion == EnumGenAsyncVersion.New)
                //{
                //    thriftProxyBuilder.Append("    <dependencies>");
                //    thriftProxyBuilder.Append(Environment.NewLine);
                //    thriftProxyBuilder.Append("      <dependency id=\"thrift_client\" version=\"2.1.3\" />");
                //    thriftProxyBuilder.Append(Environment.NewLine);
                //    thriftProxyBuilder.Append("    </dependencies>");
                //    thriftProxyBuilder.Append(Environment.NewLine);
                //}
                //else
                //{
                //    thriftProxyBuilder.Append("    <dependencies>");
                //    thriftProxyBuilder.Append(Environment.NewLine);
                //    thriftProxyBuilder.Append("      <dependency id=\"thrift_client\" version=\"1.0.0.1\" />");
                //    thriftProxyBuilder.Append(Environment.NewLine);
                //    thriftProxyBuilder.Append("    </dependencies>");
                //    thriftProxyBuilder.Append(Environment.NewLine);
                //}

                thriftProxyBuilder.Append("  </metadata>");
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("  <files>");
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("    <file src=\"" + csNamespace + ".dll.config\" target=\"/content/dllconfigs/" + csNamespace + ".dll.config\" />");
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("    <file src=\"" + csNamespace + ".dll\" target=\"/lib/net4.0/" + csNamespace + ".dll\" />");

                // XML
                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("    <file src=\"" + csNamespace + ".XML\" target=\"/lib/net4.0/" + csNamespace + ".XML\" />");

                thriftProxyBuilder.Append(Environment.NewLine);
                thriftProxyBuilder.Append("  </files>");
                thriftProxyBuilder.Append(Environment.NewLine);

                thriftProxyBuilder.Append("</package>");
                thriftProxyBuilder.Append(Environment.NewLine);

                return thriftProxyBuilder.ToString();
            }
            return string.Empty;
        }

        #region Private Method
        /// <summary>
        /// 依赖
        /// </summary>
        private static void LoadDependencies()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(ErrorResolveEventHandler);
        }
        /// <summary>
        /// 依赖错误回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Assembly ErrorResolveEventHandler(object sender, ResolveEventArgs args)
        {
            if (File.Exists(ThriftGlobal.ThriftDllPath))
            {
                return Assembly.LoadFile(ThriftGlobal.ThriftDllPath);
            }
            return null;
        }

        /// <summary>
        /// 返回service的dllconfig
        /// </summary>
        /// <param name="interfaces"></param>
        /// <returns></returns>
        private static Dictionary<string, string> MakeDllconfig(IEnumerable<Type> interfaces)
        {
            var result = new Dictionary<string, string>();
            // 输出zookeeper.config
            var zookeeperBuilder = new StringBuilder();
            zookeeperBuilder.Append("<?xml version=\"1.0\"?>");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("<configuration>");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("\t");
            zookeeperBuilder.Append("<configSections>");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("\t\t");
            zookeeperBuilder.Append("<section name=\"zookeeper\" type=\"Sodao.Zookeeper.Config.ZookeeperConfig, Zookeeper\"/>");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("\t");
            zookeeperBuilder.Append("</configSections>");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("\t");
            zookeeperBuilder.Append("<zookeeper>");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("\t\t");
            zookeeperBuilder.Append("<clients>");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("\t\t\t");
            zookeeperBuilder.Append("<client name=\"thrift\" chroot=\"/dubbo\" sessionTimeout=\"10000\" connectionString=\"127.0.0.1:2181\" />");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("\t\t");
            zookeeperBuilder.Append("</clients>");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("\t");
            zookeeperBuilder.Append("</zookeeper>");
            zookeeperBuilder.Append(Environment.NewLine);
            zookeeperBuilder.Append("</configuration>");
            result.Add("zookeeper.config", zookeeperBuilder.ToString());

            int i = 0;
            foreach (var type in interfaces)
            {
                i++;
                string declaringName = string.Empty;
                if (type.DeclaringType != null)
                    declaringName = type.DeclaringType.Name;

                // 生成client
                var clientBuilder = new StringBuilder();
                clientBuilder.Append("<?xml version=\"1.0\"?>");
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("<configuration>");
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("\t");
                clientBuilder.Append("<configSections>");
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("\t\t");
                clientBuilder.Append("<!--thrift client config-->");
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("\t\t");
                clientBuilder.Append(string.Format("<section name=\"thriftClient\" type=\"Thrift.Client.Config.ThriftConfigSection, Thrift.Client{0}\"/>", ThriftGlobal.ThriftVersion));
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("\t");
                clientBuilder.Append("</configSections>");
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("\t");
                clientBuilder.Append("<thriftClient>");
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("\t\t");
                clientBuilder.Append("<services>");


                if (ThriftGlobal.GenAsyncVersion == EnumGenAsyncVersion.New)
                {
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t");
                    clientBuilder.Append(string.Format(
                            "<service name=\"" + declaringName + "\" client=\"{0}+Client,{1}\" socketBufferSize=\"8192\" messageBufferSize=\"8192\" sendTimeout=\"3000\" receiveTimeout=\"3000\">",
                            type.Namespace + ".Async" + declaringName, type.Namespace));
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t");
                    clientBuilder.Append("<discovery>");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("<!--1:直接server配置-->");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("<servers>");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("<!--put you server here-->");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("<server host=\"127.0.0.1\" port=\"8400\"/>");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("</servers>");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("<!--2:zookeeper发现-->");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("<!--<zookeeper path=\"zookeeper.config\" name=\"thrift\" znode=\"com.sodao.demo.xxxservice\"/>-->");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t");
                    clientBuilder.Append("</discovery>");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t");
                    clientBuilder.Append("</service>");
                    clientBuilder.Append(Environment.NewLine);
                }
                else
                {
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t");
                    clientBuilder.Append(string.Format(
                            "<service name=\"" + declaringName + "\" client=\"{0}+Client,{1}\" socketBufferSize=\"8192\" messageBufferSize=\"8192\" sendReceiveTimeout=\"3000\">",
                            type.Namespace + ".Async" + declaringName, type.Namespace));
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t");
                    clientBuilder.Append("<servers>");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("<!--put you server here-->");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("<server host=\"127.0.0.1\" port=\"8400\"/>");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t\t\t");
                    clientBuilder.Append("</servers>");
                    clientBuilder.Append(Environment.NewLine);
                    clientBuilder.Append("\t\t\t");
                    clientBuilder.Append("</service>");
                    clientBuilder.Append(Environment.NewLine);
                }

                clientBuilder.Append("\t\t");
                clientBuilder.Append("</services>");
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("\t");
                clientBuilder.Append("</thriftClient>");
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("\t");
                clientBuilder.Append("<!--thrift client config end-->");
                clientBuilder.Append(Environment.NewLine);
                clientBuilder.Append("</configuration>");

                result.Add(string.Concat("client", i.ToString(), ".config"), clientBuilder.ToString());


                // 生成server
                var serverBuilder = new StringBuilder();
                serverBuilder.Append("<?xml version=\"1.0\"?>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("<configuration>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t");
                serverBuilder.Append("<configSections>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t\t");
                serverBuilder.Append("<!--thrift server config-->");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t\t");
                serverBuilder.Append(string.Format("<section name=\"thriftServer\" type=\"Thrift.Server.Config.ThriftConfigSection, Thrift.Server{0}\"/>", ThriftGlobal.ThriftVersion));
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t");
                serverBuilder.Append("</configSections>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t");
                serverBuilder.Append("<thriftServer>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t\t");
                serverBuilder.Append("<services>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t\t\t");
                serverBuilder.Append(string.Format(
                    "<service name=\"" + declaringName + "\" port=\"10000\" socketBufferSize=\"8192\" messageBufferSize=\"8192\" maxMessageSize=\"102400\" maxConnections=\"2000\" serviceType=\"\" processorType=\"{0}+Processor,{1}\">",
                        type.Namespace + ".Async" + declaringName, type.Namespace));
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t\t\t\t");
                serverBuilder.Append("<registry>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t\t\t\t\t");
                serverBuilder.Append("<zookeeper path=\"zookeeper.config\" name=\"thrift\" owner=\"owner\" znode=\"com.sodao.demo.xxxservice\"/>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t\t\t\t");
                serverBuilder.Append("</registry>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t\t\t");
                serverBuilder.Append("</service>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t\t");
                serverBuilder.Append("</services>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t");
                serverBuilder.Append("</thriftServer>");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("\t");
                serverBuilder.Append("<!--thrift server config end-->");
                serverBuilder.Append(Environment.NewLine);
                serverBuilder.Append("</configuration>");

                result.Add(string.Concat("server", i.ToString(), ".config"), serverBuilder.ToString());
            }
            return result;
        }

        /// <summary>
        /// 生成异步代理类
        /// </summary>
        /// <param name="version"></param>
        /// <param name="interfaces"></param>
        private static List<CodeResult> Build(string version, IEnumerable<Type> interfaces, XmlDocument dllXml = null)
        {
            ICodeGenerator generator = null;
            switch (version)
            {
                //case "3.5":
                //    generator = new Net35CodeGenerator();
                //    break;
                case "4.0":
                    if (ThriftGlobal.GenAsyncVersion == EnumGenAsyncVersion.New)
                        generator = new Net4CodeGenerator();
                    else
                        generator = new Net4CodeGeneratorOld();
                    break;
            }
            if (generator == null)
                return null;

            var result = generator.Generate(GetThriftTemplate(interfaces, dllXml));
            if (result != null)
            {
                return result.Select(c => new CodeResult()
                {
                    Name = "Async" + (string.IsNullOrEmpty(c.DeclaringClassName)
                            ? c.FullName
                            : c.DeclaringClassName) + "-" + "4.0" + ".cs",
                    Code = c.Code
                }).ToList();
            }
            return null;
        }

        /// <summary>
        /// 根据相关的接口生成代码模板
        /// </summary>
        /// <param name="interfaces"></param>
        /// <returns></returns>
        private static ThriftTemplate GetThriftTemplate(IEnumerable<Type> interfaces, XmlDocument dllXml = null)
        {
            var objThrift = new ThriftTemplate();
            foreach (var childType in interfaces)
            {
                string declaringName = string.Empty;
                if (childType.DeclaringType != null)
                    declaringName = childType.DeclaringType.Name;

                var objService = new ThriftTemplate.Service(childType.Namespace, childType.Name, childType.FullName, declaringName, childType.Assembly);
                objThrift.AddService(objService);
                var methods = childType.GetMethods();
                foreach (var m in methods)
                {
                    var listParams = new List<string>();
                    // yaofeng add
                    var listTypes = new List<string>();
                    var parameters = m.GetParameters();
                    if (parameters != null && parameters.Length > 0)
                    {
                        foreach (var p in parameters)
                        {
                            listParams.Add(GetTypeName(p.ParameterType) + " " + p.Name);

                            #region 从xml文件中获取summary yaofeng add
                            listTypes.Add(GetTypeName(p.ParameterType));
                            #endregion
                        }
                    }

                    #region 从xml文件中获取summary yaofeng add
                    var summary = string.Empty;
                    if (dllXml != null)
                    {
                        var name = childType.Namespace + "." + declaringName + "." + childType.Name + "." + m.Name;
                        if (listTypes.Count > 0)
                            name = name + "(" + string.Join(",", listTypes) + ")";

                        var node = dllXml.SelectSingleNode("//member[@name=\"M:" + name + "\"]");
                        if (node != null)
                        {
                            var childNodes = node.ChildNodes;
                            var summaryXmls = new List<string>();
                            foreach (XmlNode item in childNodes)
                            {
                                var outerXmls = item.OuterXml.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var outerXml in outerXmls)
                                {
                                    summaryXmls.Add("/// " + outerXml.TrimStart());
                                }
                            }
                            summary = string.Join("\r\n", summaryXmls);
                        }
                    }
                    #endregion

                    objService.AddOperation(new ThriftTemplate.Operation(GetTypeName(m.ReturnType), m.Name, listParams, summary));
                }
            }
            return objThrift;
        }

        /// <summary>
        /// 获取type的名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTypeName(Type type)
        {
            string str = string.Empty;
            if (type.IsGenericType)
            {
                str = type.Namespace + "." + type.Name.Substring(0, type.Name.IndexOf("`"));
                str += "<";
                var args = type.GetGenericArguments();
                string strChild = string.Empty;
                foreach (var child in args)
                {
                    strChild += GetTypeName(child) + ", ";
                }

                str += strChild.TrimEnd(' ').TrimEnd(',') + ">";
            }
            else
                str = type.FullName;

            return str;
        }
        #endregion
    }

    /// <summary>
    /// 调用反射生成的代码返回结果
    /// </summary>
    public class CodeResult
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 代码
        /// </summary>
        public string Code { get; set; }
    }
}
