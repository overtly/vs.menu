using VS.Menu.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using VS.Menu.Helper;

namespace VS.Menu.GrpcGenCore
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
        public static bool Execute(string filePath, EnumGrpcGenType genType, string tempKey, out string csNamespace, out string errorMsg, out string resultDic)
        {
            csNamespace = string.Empty;
            errorMsg = string.Empty;
            resultDic = string.Empty;
            if (!Utility.CheckFilePath(filePath))
            {
                errorMsg = "没有文件可以生成";
                return false;
            }

            var csFiles = GenUtilityOrigin.GenGrpc(filePath, out errorMsg, out csNamespace);
            if (!string.IsNullOrEmpty(errorMsg))
                return false;

            if (csFiles == null || csFiles.Length == 0)
            {
                errorMsg = "proto 协议文件有异常";
                return false;
            }
            resultDic = csFiles[0].Directory.FullName;

            // 生成配置文件
            var fmConfigs = GenUtilityOrigin.MakeFmConfig();
            if (fmConfigs != null && fmConfigs.Count > 0)
            {
                var fmConfigDic = Path.Combine(resultDic, "fmconfigs");
                Utility.MakeDir(fmConfigDic);

                foreach (var configPair in fmConfigs)
                {
                    var dllconfigFileName = Path.Combine(fmConfigDic, configPair.Key);
                    Utility.WriteNewFile(dllconfigFileName, configPair.Value);
                }
            }

            var coreConfigs = GenUtilityOrigin.MakeCoreConfig();
            if (fmConfigs != null && fmConfigs.Count > 0)
            {
                var coreConfigDic = Path.Combine(resultDic, "coreconfigs");
                Utility.MakeDir(coreConfigDic);

                foreach (var configPair in coreConfigs)
                {
                    var dllconfigFileName = Path.Combine(coreConfigDic, configPair.Key);
                    Utility.WriteNewFile(dllconfigFileName, configPair.Value);
                }
            }


            //如果只需要生成原生的，就直接返回
            if (genType == EnumGrpcGenType.Origin)
                return true;

            #region 第一次编译
            // 每次生成都只能生成一次文件夹路径
            var fileName = new FileInfo(filePath).Name;
            var projDicPath = GrpcGlobal.GenProjDic(fileName);
            if (string.IsNullOrEmpty(projDicPath))
            {
                errorMsg = "Create gen temp folder error,may u have already open it!";
                return false;
            }

            // 组织这些文件成为一个project.xml文件
            var projXml = BuildGrpcProject.MakeProj(csFiles, "grpcProj");
            var projXmlPath = Path.Combine(projDicPath, "grpcProj.csproj");
            Utility.WriteNewFile(projXmlPath, projXml);

            // 拷贝json文件
            GrpcGlobal.MoveProjectAsset(fileName);

            // 调用MSBuild生成这个项目
            var msbuildPath = GrpcGlobal.MsBuildPath();
            if (string.IsNullOrEmpty(msbuildPath))
            {
                errorMsg = "VS2017安装目录中找不到MSBuild.exe，或者安装的VS2017目录没有2017标识，或者VS2017从未打开过, 请检查注册表 \r\n Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache";
                return false;
            }

            // 调用restore
            Utility.RunCmd("dotnet", "restore", projDicPath, out errorMsg);

            // 为防止有的客户端路径中包含空格影响参数的设置
            // 设置运行的目录在客户端当前目录调用MSBuild
            Utility.RunProcess(msbuildPath, projDicPath, "grpcProj.csproj");
            #endregion

            #region 复制到专门的路径
            var net46Fold = Path.Combine(projDicPath, @"bin\Debug\net46");
            var assemblyDic = GrpcGlobal.GetLoadAssemblyDic(fileName, tempKey);
            Utility.Copy(net46Fold, assemblyDic);
            #endregion

            // 获取命名空间
            var grpcDllPath = Path.Combine(assemblyDic, "net46", "grpcProj.dll");

            // 创建一个ClientManager
            var proxyCode = GenUtilityOrigin.GenAsyncProxyCs(grpcDllPath, out csNamespace);
            var proxyFilePath = Path.Combine(resultDic, "ClientManager.cs");
            Utility.WriteNewFile(proxyFilePath, proxyCode);

            // 如果只需要生成代码
            if(genType == EnumGrpcGenType.Client)
                return true;

            #region 第二次编译
            // 每次生成都只能生成一次文件夹路径
            fileName = new FileInfo(filePath).Name;
            projDicPath = GrpcGlobal.GenProjDic(fileName);
            if (string.IsNullOrEmpty(projDicPath))
            {
                errorMsg = "Create gen temp folder error,may u have already open it!";
                return false;
            }

            // 组织这些文件成为一个project.xml文件
            csFiles = new DirectoryInfo(resultDic).GetFiles("*.cs");
            projXml = BuildGrpcProject.MakeProj(csFiles, csNamespace);
            projXmlPath = Path.Combine(projDicPath, csNamespace + ".csproj");
            Utility.WriteNewFile(projXmlPath, projXml);

            // 拷贝json文件
            GrpcGlobal.MoveProjectAsset(fileName);

            // 调用MSBuild生成这个项目
            msbuildPath = GrpcGlobal.MsBuildPath();
            if (string.IsNullOrEmpty(msbuildPath))
            {
                errorMsg = "VS2017安装目录中找不到MSBuild.exe，或者安装的VS2017目录没有2017标识，或者VS2017从未打开过, 请检查注册表 \r\n Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache";
                return false;
            }

            // 调用restore
            Utility.RunCmd("dotnet", "restore", projDicPath, out errorMsg);


            // 为防止有的客户端路径中包含空格影响参数的设置
            // 设置运行的目录在客户端当前目录调用MSBuild
            Utility.RunProcess(msbuildPath, projDicPath, csNamespace + ".csproj");
            #endregion

            grpcDllPath = Path.Combine(projDicPath, @"bin\Debug\net46", csNamespace + ".dll");
            resultDic = GenUtilityOrigin.ToResultDic(fileName, grpcDllPath, csNamespace, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 生成服务端dll
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="genType"></param>
        /// <param name="errorMsg"></param>
        /// <param name="resultDic"></param>
        /// <returns></returns>
        public static bool ExecuteDll(string filePath, string tempKey, out string csNamespace, out string errorMsg, out string resultDic)
        {
            csNamespace = string.Empty;
            errorMsg = string.Empty;
            resultDic = string.Empty;

            if (!Execute(filePath, EnumGrpcGenType.GenDll, tempKey, out csNamespace, out errorMsg, out resultDic))
            {
                return false;
            }

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
        public static bool ExcuteNuget(string filePath, ServiceModel serviceModel, string tempKey, out string csNamespace, out string errorMsg, out string resultDic)
        {
            csNamespace = string.Empty;
            errorMsg = string.Empty;
            resultDic = string.Empty;

            if (!ExecuteDll(filePath, tempKey, out csNamespace, out errorMsg, out resultDic))
            {
                return false;
            }

            var nuspecXml = GenUtilityOrigin.GenNuspecXml(resultDic, serviceModel, csNamespace);
            var nuspecFilePath = Path.Combine(resultDic, "Package.nuspec");
            Utility.WriteNewFile(nuspecFilePath, nuspecXml);

            // 生成nuget包msbu
            var error = string.Empty;
            Utility.RunCmd(Utility.NugetExePath, "pack " + nuspecFilePath + " -OutputDirectory " + resultDic, out error);

            // 直接发布
            if (serviceModel.Publish)
            {
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


        /// <summary>
        /// 生成异步或者通过的代码文件
        /// </summary>
        /// <param name="fullFilePathList"></param>
        public static bool Execute_GrpcNet(string filePath, EnumGrpcGenType genType, string tempKey, out string csNamespace, out string errorMsg, out string resultDic)
        {
            csNamespace = string.Empty;
            errorMsg = string.Empty;
            resultDic = string.Empty;
            if (!Utility.CheckFilePath(filePath))
            {
                errorMsg = "没有文件可以生成";
                return false;
            }

            var csFiles = GenUtilityOrigin.GenGrpc(filePath, out errorMsg, out csNamespace);
            if (!string.IsNullOrEmpty(errorMsg))
                return false;

            if (csFiles == null || csFiles.Length == 0)
            {
                errorMsg = "proto 协议文件有异常";
                return false;
            }
            resultDic = csFiles[0].Directory.FullName;

            // 生成配置文件
            var coreConfigs = GenUtilityOrigin.MakeCoreConfig();
            var coreConfigDic = Path.Combine(resultDic, "coreconfigs");
            Utility.MakeDir(coreConfigDic);

            foreach (var configPair in coreConfigs)
            {
                var dllconfigFileName = Path.Combine(coreConfigDic, configPair.Key);
                Utility.WriteNewFile(dllconfigFileName, configPair.Value);
            }

            //如果只需要生成原生的，就直接返回
            if (genType == EnumGrpcGenType.Origin)
                return true;

            #region 第二次编译
            // 每次生成都只能生成一次文件夹路径
            var fileName = new FileInfo(filePath).Name;
            var projDicPath = GrpcGlobal.GenProjDic(fileName);
            if (string.IsNullOrEmpty(projDicPath))
            {
                errorMsg = "Create gen temp folder error,may u have already open it!";
                return false;
            }

            // 组织这些文件成为一个project.xml文件
            csFiles = new DirectoryInfo(resultDic).GetFiles("*.cs");
            var projXml = BuildGrpcProject.MakeProj(csFiles, csNamespace, "netstandard2.1;netcoreapp3.0;net5.0", "grpch2");
            var projXmlPath = Path.Combine(projDicPath, csNamespace + ".csproj");
            Utility.WriteNewFile(projXmlPath, projXml);

            // 拷贝json文件
            GrpcGlobal.MoveProjectAsset(fileName);

            // 调用MSBuild生成这个项目
            var msbuildPath = GrpcGlobal.MsBuildPath();
            if (string.IsNullOrEmpty(msbuildPath))
            {
                errorMsg = "VS2017安装目录中找不到MSBuild.exe，或者安装的VS2017目录没有2017标识，或者VS2017从未打开过, 请检查注册表 \r\n Software\\Classes\\Local Settings\\Software\\Microsoft\\Windows\\Shell\\MuiCache";
                return false;
            }

            // 调用restore
            Utility.RunCmd("dotnet", "restore", projDicPath, out errorMsg);


            // 为防止有的客户端路径中包含空格影响参数的设置
            // 设置运行的目录在客户端当前目录调用MSBuild
            Utility.RunProcess(msbuildPath, projDicPath, csNamespace + ".csproj");
            #endregion

            var grpcDllPath = Path.Combine(projDicPath, @"bin\Debug\netstandard2.1", csNamespace + ".dll");
            resultDic = GenUtilityOrigin_GrpcNet.ToResultDic(fileName, grpcDllPath, csNamespace, out errorMsg);
            if (!string.IsNullOrEmpty(errorMsg))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 生成服务端dll
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="genType"></param>
        /// <param name="errorMsg"></param>
        /// <param name="resultDic"></param>
        /// <returns></returns>
        public static bool ExecuteDll_GrpcNet(string filePath, string tempKey, out string csNamespace, out string errorMsg, out string resultDic)
        {
            csNamespace = string.Empty;
            errorMsg = string.Empty;
            resultDic = string.Empty;

            if (!Execute_GrpcNet(filePath, EnumGrpcGenType.GenDll, tempKey, out csNamespace, out errorMsg, out resultDic))
            {
                return false;
            }

            return true;
        }

        public static bool ExecueNuget_GrpcNet(string filePath, ServiceModel serviceModel, string tempKey, out string csNamespace, out string errorMsg, out string resultDic)
        {
            csNamespace = string.Empty;
            errorMsg = string.Empty;
            resultDic = string.Empty;

            if (!ExecuteDll_GrpcNet(filePath, tempKey, out csNamespace, out errorMsg, out resultDic))
            {
                return false;
            }

            var nuspecXml = GenUtilityOrigin_GrpcNet.GenNuspecXml(resultDic, serviceModel, csNamespace);
            var nuspecFilePath = Path.Combine(resultDic, "Package.nuspec");
            Utility.WriteNewFile(nuspecFilePath, nuspecXml);

            // 生成nuget包msbu
            var error = string.Empty;
            Utility.RunCmd(Utility.NugetExePath, "pack " + nuspecFilePath + " -OutputDirectory " + resultDic, out error);

            // 直接发布
            if (serviceModel.Publish)
            {
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