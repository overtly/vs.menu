using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VS.Menu.Helper;

namespace VS.Menu.GrpcGenCore
{
    /// <summary>
    /// 创建工程的应用类
    /// </summary>
    public class BuildGrpcProject
    {
        /// <summary>
        /// 创建项目xml文件
        /// </summary>
        public static string MakeProj(IEnumerable<FileInfo> includeFiles, string csNamespace)
        {
            var builder = new StringBuilder();
            AppenProjectHead(builder);
            AppendProperty(builder, csNamespace);
            AppendReference(builder);
            AppendInclude(builder, includeFiles);
            AppendProjectTial(builder);
            return builder.ToString();
        }


        #region Project Xml Build
        /// <summary>
        /// 添加project头的xml标签
        /// </summary>
        /// <param name="builder"></param>
        private static void AppenProjectHead(StringBuilder builder)
        {
            builder.Append("<Project Sdk=\"Microsoft.NET.Sdk\">");
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
            builder.Append("<TargetFrameworks>net45;net46;netstandard2.0</TargetFrameworks>");
            builder.Append(Environment.NewLine);
            builder.Append("</PropertyGroup>");
            builder.Append(Environment.NewLine);

            builder.Append("<PropertyGroup>");
            builder.Append(Environment.NewLine);
            builder.Append("<DocumentationFile>" + csNamespace + ".xml</DocumentationFile>");
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

            // 依赖
            var type = $"grpc";
            var dependencies = DependenceHelper.Get(type);
            foreach (var dependence in dependencies)
            {
                builder.Append($"<PackageReference Include=\"{dependence.PackageId}\" Version=\"{dependence.Version}\" />");
                builder.Append(Environment.NewLine);
            }

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
        }
        #endregion
    }
}