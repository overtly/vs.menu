using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VS.Menu.ThriftGenCore
{
    /// <summary>
    /// 创建工程的应用类
    /// </summary>
    public class BuildThriftProject
    {
        /// <summary>
        /// 创建项目xml文件
        /// </summary>
        public static string MakeProjXml(IEnumerable<FileInfo> includeFiles, string csNamespace)
        {
            var builder = new StringBuilder();
            AppendXmlHead(builder);
            AppenProjectHead(builder);
            AppendProperty(builder, csNamespace);
            AppendReference(builder);
            AppendInclude(builder, includeFiles);
            AppendProjectTial(builder);
            return builder.ToString();
        }


        /// <summary>
        /// 创建项目xml文件
        /// </summary>
        public static string MakeClientProjXml(IEnumerable<FileInfo> includeFiles, string csNamespace)
        {
            var builder = new StringBuilder();
            AppendXmlHead(builder);
            AppenProjectHead(builder);
            AppendProperty(builder, csNamespace);
            AppendClientReference(builder);
            AppendInclude(builder, includeFiles);
            AppendProjectTial(builder);
            return builder.ToString();
        }


        /// <summary>
        /// 创建项目xml文件
        /// </summary>
        public static string MakeServerProjXml(IEnumerable<FileInfo> includeFiles, string csNamespace)
        {
            var builder = new StringBuilder();
            AppendXmlHead(builder);
            AppenProjectHead(builder);
            AppendProperty(builder, csNamespace);
            AppendServerReference(builder);
            AppendInclude(builder, includeFiles);
            AppendProjectTial(builder);
            return builder.ToString();
        }



        #region Project Xml Build
        /// <summary>
        /// 添加xml头
        /// </summary>
        /// <param name="builder"></param>
        private static void AppendXmlHead(StringBuilder builder)
        {
            builder.Append(@"<?xml version='1.0' encoding='utf-8'?>");
            builder.Append(Environment.NewLine);
        }

        /// <summary>
        /// 添加project头的xml标签
        /// </summary>
        /// <param name="builder"></param>
        private static void AppenProjectHead(StringBuilder builder)
        {
            builder.Append(
                "<Project ToolsVersion='12.0' DefaultTargets='Build' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>");
            builder.Append(Environment.NewLine);
        }

        /// <summary>
        /// 添加project结束标签
        /// </summary>
        /// <param name="builder"></param>
        private static void AppendProjectTial(StringBuilder builder)
        {
            builder.Append("</Project>");
            builder.Append(Environment.NewLine);
        }

        /// <summary>
        /// 添加属性的xml标签
        /// </summary>
        /// <param name="builder"></param>
        private static void AppendProperty(StringBuilder builder, string csNamespace = "")
        {
            builder.Append("<PropertyGroup>");
            builder.Append(Environment.NewLine);
            builder.Append("<Configuration Condition=\" '$(Configuration)' == '' \">Debug</Configuration>");
            builder.Append(Environment.NewLine);
            builder.Append("<Platform Condition=\" '$(Platform)' == '' \">AnyCPU</Platform>");
            builder.Append(Environment.NewLine);
            builder.Append("<ProductVersion>1.0.0</ProductVersion>");
            builder.Append(Environment.NewLine);
            builder.Append("<OutputType>Library</OutputType>");
            builder.Append(Environment.NewLine);
            builder.Append("<AppDesignerFolder>Properties</AppDesignerFolder>");
            builder.Append(Environment.NewLine);
            builder.Append("<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>");
            builder.Append(Environment.NewLine);
            builder.Append("<FileAlignment>512</FileAlignment>");
            builder.Append(Environment.NewLine);
            builder.Append("</PropertyGroup>");
            builder.Append(Environment.NewLine);
            builder.Append("<PropertyGroup Condition=\" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' \">");
            builder.Append(Environment.NewLine);
            builder.Append("<DebugSymbols>false</DebugSymbols>");
            builder.Append(Environment.NewLine);
            // 生成pdb文件
            //builder.Append("<DebugType>full</DebugType>");
            //builder.Append(Environment.NewLine);
            builder.Append("<Optimize>false</Optimize>");
            builder.Append(Environment.NewLine);
            builder.Append("<OutputPath>bin</OutputPath>");
            builder.Append(Environment.NewLine);
            builder.Append("<DefineConstants>DEBUG</DefineConstants>");
            builder.Append(Environment.NewLine);
            builder.Append("<ErrorReport>prompt</ErrorReport>");
            builder.Append(Environment.NewLine);
            builder.Append("<WarningLevel>4</WarningLevel>");
            // 生成xml文件
            if (!string.IsNullOrEmpty(csNamespace))
            {
                builder.Append(Environment.NewLine);
                builder.Append("<DocumentationFile>bin\\" + csNamespace + ".XML</DocumentationFile>");
            }

            builder.Append(Environment.NewLine);
            builder.Append("</PropertyGroup>");
            builder.Append(Environment.NewLine);
        }

        /// <summary>
        /// 添加引用的xml标签
        /// </summary>
        /// <param name="builder"></param>
        private static void AppendReference(StringBuilder builder)
        {
            builder.Append("<ItemGroup>");
            builder.Append(Environment.NewLine);
            builder.Append("<Reference Include=\"Thrift\">");
            builder.Append(Environment.NewLine);
            // 这里的dll是作为内容生成在应用目录下的
            builder.Append(string.Format("<HintPath>{0}</HintPath>", ThriftGlobal.ThriftDllPath));
            builder.Append(Environment.NewLine);
            builder.Append("</Reference>");
            builder.Append(Environment.NewLine);
            builder.Append("</ItemGroup>");
            builder.Append(Environment.NewLine);
        }

        /// <summary>
        /// 添加包含文件的xml标签
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="includeFiles"></param>
        private static void AppendInclude(StringBuilder builder, IEnumerable<FileInfo> includeFiles)
        {
            builder.Append("<ItemGroup>");
            builder.Append(Environment.NewLine);

            foreach (var file in includeFiles)
            {
                var isCsFile = file.Extension == ".cs";
                builder.Append(string.Format("<{0} Include=\"{1}\" />", isCsFile ? "Compile" : "None", file.FullName));
                builder.Append(Environment.NewLine);
            }
            builder.Append("</ItemGroup>");
            builder.Append(Environment.NewLine);
            builder.Append("<Import Project=\"$(MSBuildToolsPath)\\Microsoft.CSharp.targets\" />  ");
            builder.Append(Environment.NewLine);
        }
        #endregion


        #region Server Project Xml Build
        /// <summary>
        /// 客户端依赖
        /// </summary>
        /// <param name="builder"></param>
        private static void AppendServerReference(StringBuilder builder)
        {
            builder.Append("<ItemGroup>");
            builder.Append(Environment.NewLine);

            builder.Append("<Reference Include=\"Thrift\">");
            builder.Append(Environment.NewLine);
            builder.Append(string.Format("<HintPath>{0}</HintPath>", ThriftGlobal.ThriftDllPath));
            builder.Append(Environment.NewLine);
            builder.Append("</Reference>");
            builder.Append(Environment.NewLine);

            builder.Append("</ItemGroup>");
            builder.Append(Environment.NewLine);
        }
        #endregion


        #region Client Project Xml Build
        /// <summary>
        /// 客户端依赖
        /// </summary>
        /// <param name="builder"></param>
        private static void AppendClientReference(StringBuilder builder)
        {
            builder.Append("<ItemGroup>");
            builder.Append(Environment.NewLine);
            builder.Append("<Reference Include=\"FastSocket.Client\">");
            builder.Append(Environment.NewLine);
            builder.Append(string.Format("<HintPath>{0}</HintPath>", ThriftGlobal.FastSocketClientDllPath));
            builder.Append(Environment.NewLine);
            builder.Append("</Reference>");
            builder.Append(Environment.NewLine);

            builder.Append("<Reference Include=\"FastSocket.SocketBase\">");
            builder.Append(Environment.NewLine);
            builder.Append(string.Format("<HintPath>{0}</HintPath>", ThriftGlobal.FastSocketSocketBaseDllPath));
            builder.Append(Environment.NewLine);
            builder.Append("</Reference>");
            builder.Append(Environment.NewLine);

            builder.Append("<Reference Include=\"Thrift.Client\">");
            builder.Append(Environment.NewLine);
            builder.Append(string.Format("<HintPath>{0}</HintPath>", ThriftGlobal.ThriftClientDllPath));
            builder.Append(Environment.NewLine);
            builder.Append("</Reference>");
            builder.Append(Environment.NewLine);

            builder.Append("<Reference Include=\"Thrift\">");
            builder.Append(Environment.NewLine);
            builder.Append(string.Format("<HintPath>{0}</HintPath>", ThriftGlobal.ThriftDllPath));
            builder.Append(Environment.NewLine);
            builder.Append("</Reference>");
            builder.Append(Environment.NewLine);

            builder.Append("<Reference Include=\"Zookeeper\">");
            builder.Append(Environment.NewLine);
            builder.Append(string.Format("<HintPath>{0}</HintPath>", ThriftGlobal.ZookeeperDllPath));
            builder.Append(Environment.NewLine);
            builder.Append("</Reference>");
            builder.Append(Environment.NewLine);

            builder.Append("</ItemGroup>");
            builder.Append(Environment.NewLine);
        }
        #endregion
    }
}