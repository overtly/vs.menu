using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using VS.Menu.Helper;
using VS.Menu.Model;

namespace VS.Menu.ThriftGenCore
{
    /// <summary>
    /// 执行生成的执行器
    /// </summary>
    public class GenExecutor
    {
        /// <summary>
        /// 生成异步或者通过的代码文件
        /// </summary>
        /// <param name="fullFilePathList"></param>
        public static bool Execute(string filePath, EnumGenType genType, out string errorMsg, out string resultDir)
        {
            errorMsg = string.Empty;
            resultDir = string.Empty;
            if (!Utility.CheckFilePath(filePath))
            {
                errorMsg = "没有文件可以生成";
                return false;
            }

            string fileName = new FileInfo(filePath).Name;

            // 该生成的结果文件夹
            resultDir = ThriftGlobal.GenResultDic(fileName);

            // 调用thrift命令生成cs文件
            string output;
            FileInfo[] csFiles = GenUtilityOrigin.GenThrift(filePath, resultDir, out output, out errorMsg);
            if (!string.IsNullOrEmpty(output))
            {
                var showMsg = MessageBox.Show(output + "!!! 有警告是否继续?", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (showMsg == MessageBoxResult.No)
                {
                    errorMsg = "已中止，请更改对应的错误模块";
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(errorMsg))
            {
                return false;
            }

            if (csFiles == null || csFiles.Length == 0)
            {
                errorMsg = "Thrift -gen csharp make file failure,maybe ur .thrift file has errors";
                return false;
            }

            //如果只需要生成原生的，就直接返回
            if (genType == EnumGenType.Origin)
            {
                resultDir = csFiles[0].Directory.FullName;
                return true;
            }

            // 每次生成都只能生成一次文件夹路径
            string projDicPath = ThriftGlobal.GenProjDic(fileName);
            if (string.IsNullOrEmpty(projDicPath))
            {
                errorMsg = "Create gen temp folder error,may u have already open it!";
                return false;
            }

            // 组织这些文件成为一个project.xml文件
            var projXml = BuildThriftProject.MakeProjXml(csFiles, "thriftProj");
            string thriftProjFilePath = Path.Combine(projDicPath, "thriftProj.xml");
            Utility.WriteNewFile(thriftProjFilePath, projXml);

            // 调用MSBuild生成这个项目
            var msbuildPath = ThriftGlobal.MsBuildPath();
            if (string.IsNullOrEmpty(msbuildPath))
            {
                errorMsg = "Can not find MsBuild,have u install .net framework 4?";
                return false;
            }
            // 为防止有的客户端路径中包含空格影响参数的设置
            // 设置运行的目录在客户端当前目录调用MSBuild
            Utility.RunProcess(msbuildPath, projDicPath, "thriftProj.xml");

            // 获取到生成的thrift源文件的dll
            var thriftDllPath = Path.Combine(projDicPath, "bin", "thriftProj.dll");
            var thriftDllXmlPath = Path.Combine(projDicPath, "bin", "thriftProj.XML");
            if (!File.Exists(thriftDllPath))
            {
                // MSBuild生成thrift源文件的项目失败
                errorMsg = "Something wrong,Plz call Oliver";
                return false;
            }

            // 判断命名空间是否符合规则
            var csNamespace = GenUtilityAsync.GetNamespace(thriftDllPath);
            if (string.IsNullOrEmpty(csNamespace))
            {
                errorMsg = "Namespace not exist";
                return false;
            }
            var namespaceRegex = new Regex(@"^(([A-Z][a-zA-Z]{1,}\.)+)Thrift$");
            if (!namespaceRegex.IsMatch(csNamespace))
            {
                errorMsg = "Namespace not match [Xyyyy.Thrift]";
                var showMsg = MessageBox.Show(errorMsg + "!!! 是否继续?", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (showMsg == MessageBoxResult.No)
                {
                    errorMsg = "已中止，请更改未符合规则的命名空间名称";
                    return false;
                }
            }

            // 调用反射生成异步的代理类
            Dictionary<string, string> dllConfigs;
            var codeResults = GenUtilityAsync.GenAsync(thriftDllPath, thriftDllXmlPath, out dllConfigs);
            if (codeResults == null || codeResults.Count == 0)
            {
                // MSBuild生成thrift源文件的项目失败
                errorMsg = "No Service Found!!!";
                return false;
            }
            // 把codeResult输出成文件
            resultDir = Path.Combine(resultDir, "gen-csharp-latest");
            if (!Utility.CheckDicPath(resultDir))
                Utility.MakeDir(resultDir);

            foreach (var codeResult in codeResults)
            {
                var asyncFileName = Path.Combine(resultDir, codeResult.Name);
                Utility.WriteNewFile(asyncFileName, codeResult.Code);
            }
            foreach (var fileInfo in csFiles)
            {
                fileInfo.CopyTo(Path.Combine(resultDir, fileInfo.Name), true);
            }
            // 输出client.dllconfig
            if (dllConfigs != null && dllConfigs.Count > 0)
            {
                foreach (var configPair in dllConfigs)
                {
                    var dllconfigFileName = Path.Combine(resultDir, configPair.Key);
                    Utility.WriteNewFile(dllconfigFileName, configPair.Value);
                }
            }
            return true;
        }

        /// <summary>
        /// 生成服务端dll
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="genType"></param>
        /// <param name="errorMsg"></param>
        /// <param name="resultDir"></param>
        /// <returns></returns>
        public static bool ExecuteDll(string filePath, out string errorMsg, out string resultDir)
        {
            errorMsg = string.Empty;
            resultDir = string.Empty;

            if (!Execute(filePath, EnumGenType.Async, out errorMsg, out resultDir))
            {
                return false;
            }

            string fileName = new FileInfo(filePath).Name;

            // 每次生成都只能生成一次文件夹路径
            string projServerDicPath = ThriftGlobal.GenServerProjDir(fileName);
            if (string.IsNullOrEmpty(projServerDicPath))
            {
                errorMsg = "Create gen temp folder error,may u have already open it!";
                return false;
            }

            // 创建一个AssemblyInfo文件
            var projDicPath = ThriftGlobal.GetProjDir(fileName);
            var thriftDllPath = Path.Combine(projDicPath, "bin", "thriftProj.dll");
            var csNamespace = string.Empty;
            var assemblyInfoCode = GenUtilityAsync.GenAssemblyInfoCs(thriftDllPath, out csNamespace);
            var assemblyInfoFilePath = Path.Combine(resultDir, "AssemblyInfo.cs");
            Utility.WriteNewFile(assemblyInfoFilePath, assemblyInfoCode);

            // 组织这些文件成为一个project.xml文件
            var files = new DirectoryInfo(resultDir).GetFiles();
            var projXml = BuildThriftProject.MakeServerProjXml(files, csNamespace);
            string thriftProjFilePath = Path.Combine(projServerDicPath, csNamespace + ".xml");
            Utility.WriteNewFile(thriftProjFilePath, projXml);

            // 调用MSBuild生成这个项目
            var msbuildPath = ThriftGlobal.MsBuildPath();
            if (string.IsNullOrEmpty(msbuildPath))
            {
                errorMsg = "Can not find MsBuild,have u install .net framework 4?";
                return false;
            }
            // 为防止有的客户端路径中包含空格影响参数的设置
            // 设置运行的目录在客户端当前目录调用MSBuild
            Utility.RunProcess(msbuildPath, projServerDicPath, csNamespace + ".xml");

            resultDir = Path.Combine(projServerDicPath, "bin");

            return true;
        }

        /// <summary>
        /// 生成dll
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="genType"></param>
        /// <param name="errorMsg"></param>
        /// <param name="serviceModel"></param>
        /// <returns></returns>
        public static bool ExecuteClientDll(string filePath, ServiceModel serviceModel, out string errorMsg, out string resultDic)
        {
            errorMsg = string.Empty;
            resultDic = string.Empty;
            if (serviceModel == null)
            {
                errorMsg = "服务信息数据未提供";
                return false;
            }

            if (!Execute(filePath, EnumGenType.Async, out errorMsg, out resultDic))
            {
                return false;
            }

            string fileName = new FileInfo(filePath).Name;

            // 每次生成都只能生成一次文件夹路径
            string projClientDicPath = ThriftGlobal.GenClientProjDir(fileName);
            if (string.IsNullOrEmpty(projClientDicPath))
            {
                errorMsg = "Create gen temp folder error,may u have already open it!";
                return false;
            }

            // 创建一个App.config
            var clientConfigs = new DirectoryInfo(resultDic).GetFiles("client*");
            if (clientConfigs.Length > 0)
            {
                var appConfigPath = Path.Combine(resultDic, "App.config");
                var otherConfigs = new List<FileInfo>();

                for (int i = 0; i < clientConfigs.Length; i++)
                {
                    if (i == 0)
                    {
                        clientConfigs[i].CopyTo(appConfigPath);
                        continue;
                    }
                    otherConfigs.Add(clientConfigs[i]);
                }

                ThriftGlobal.ChangeAppConfig(serviceModel, appConfigPath, otherConfigs);
            }

            // 创建一个_ThriftProxy
            var projDicPath = ThriftGlobal.GetProjDir(fileName);
            var thriftDllPath = Path.Combine(projDicPath, "bin", "thriftProj.dll");
            var csNamespace = string.Empty;
            var proxyCode = GenUtilityAsync.GenAsyncProxyCs(thriftDllPath, serviceModel, out csNamespace);
            var proxyFilePath = Path.Combine(resultDic, "_ThriftProxy.cs");
            Utility.WriteNewFile(proxyFilePath, proxyCode);

            // 创建一个AssemblyInfo文件
            var assemblyInfoCode = GenUtilityAsync.GenAssemblyInfoCs(thriftDllPath, out csNamespace);
            var assemblyInfoFilePath = Path.Combine(resultDic, "AssemblyInfo.cs");
            Utility.WriteNewFile(assemblyInfoFilePath, assemblyInfoCode);

            // 组织这些文件成为一个project.xml文件
            var files = new DirectoryInfo(resultDic).GetFiles();
            var projXml = BuildThriftProject.MakeClientProjXml(files, csNamespace);
            string thriftProjFilePath = Path.Combine(projClientDicPath, csNamespace + ".xml");
            Utility.WriteNewFile(thriftProjFilePath, projXml);

            // 调用MSBuild生成这个项目
            var msbuildPath = ThriftGlobal.MsBuildPath();
            if (string.IsNullOrEmpty(msbuildPath))
            {
                errorMsg = "Can not find MsBuild,have u install .net framework 4?";
                return false;
            }
            // 为防止有的客户端路径中包含空格影响参数的设置
            // 设置运行的目录在客户端当前目录调用MSBuild
            Utility.RunProcess(msbuildPath, projClientDicPath, csNamespace + ".xml");

            resultDic = Path.Combine(projClientDicPath, "bin");

            return true;
        }

        /// <summary>
        ///生成nuget包
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="serviceModel"></param>
        /// <param name="errorMsg"></param>
        /// <param name="resultDic"></param>
        /// <returns></returns>
        public static bool ExcuteClientNuget(string filePath, ServiceModel serviceModel, out string errorMsg, out string resultDic)
        {
            errorMsg = string.Empty;
            resultDic = string.Empty;

            if (!ExecuteClientDll(filePath, serviceModel, out errorMsg, out resultDic))
            {
                return false;
            }

            var fileName = new FileInfo(filePath).Name;
            // 生成nuget文件
            var projDicPath = ThriftGlobal.GetProjDir(fileName);
            var thriftDllPath = Path.Combine(projDicPath, "bin", "thriftProj.dll");
            var nuspecXml = GenUtilityAsync.GenNuspecXml(thriftDllPath, serviceModel.NugetId);
            if (string.IsNullOrEmpty(nuspecXml))
            {
                errorMsg = "nuspec file gen error!!!";
                return false;
            }

            var nuspecFilePath = Path.Combine(resultDic, "Package.nuspec");
            Utility.WriteNewFile(nuspecFilePath, nuspecXml);

            // 生成nuget包
            var error = string.Empty;
            Utility.RunCmd(Utility.NugetExePath, "pack " + nuspecFilePath + " -OutputDirectory " + resultDic, out error);

            // 直接发布
            if (serviceModel.Publish)
            {
                //var nugetServer = "http://10.0.60.89:8081";
                //if (ThriftGlobal.GenAsyncVersion == EnumGenAsyncVersion.Old)
                //    nugetServer = "http://10.0.60.89:8080";

                var nugetServer = NugetServerHelper.Get();
                var nugetPks = new DirectoryInfo(resultDic).GetFiles("*.nupkg");
                if (nugetPks.Length <= 0)
                {
                    errorMsg = "nupkg not gen";
                    return false;
                }

                Utility.RunCmd(Utility.NugetExePath, "push " + nugetPks[0].FullName + " -s " + nugetServer.Address + " " + nugetServer.Key, out error);
            }

            return true;
        }
    }
}