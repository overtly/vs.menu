using VS.Menu.Helper;
using VS.Menu.ThriftGenCore;
using VS.Menu.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS.Menu.ThriftGenCore
{
    internal class ThriftGenerator
    {
        ///// <summary>
        ///// 临时文件目录
        ///// </summary>
        //readonly string _TempDir = "";
        ///// <summary>
        ///// thrift 客户端地址
        ///// </summary>
        //readonly string _ThriftClient = "";
        ///// <summary>
        ///// 临时模板名称
        ///// </summary>
        //const string _TEMPLATE_NAME = "servicetemplate.thrift";
        ///// <summary>
        ///// 生成的csharp代码目录
        ///// </summary>
        //readonly string _GenCsharpDic = "";

        public static ThriftGenerator Default = new ThriftGenerator();

        public ThriftGenerator()
        {
            #region 创建临时目录
            var templateDic = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Utility.TmpFoldName);
            if (!System.IO.Directory.Exists(templateDic))
            {
                try { System.IO.Directory.CreateDirectory(templateDic); }
                catch (Exception ex) { }
                return;
            }
            #endregion

            #region 版本不同删除
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if (!VersionHelper.Compare(version))
            {
                Utility.DeleteResource();
            }
            #endregion
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="sorceFile"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool GenCsharp(string sorceFile, EnumGenType genType, out string errorMsg, ServiceModel serviceModel = null)
        {
            #region 逻辑判断
            errorMsg = string.Empty;
            if (string.IsNullOrWhiteSpace(sorceFile))
            {
                errorMsg = "请输入源文件地址";
                return false;
            }

            if (!System.IO.File.Exists(sorceFile))
            {
                errorMsg = "您输入的地址文件不存在";
                return false;
            }

            string ext = System.IO.Path.GetExtension(sorceFile).ToLower();
            if (ext != ".thrift")
            {
                errorMsg = "只支持thrift文件";
                return false;
            }
            #endregion

            try
            {
                var resultDic = string.Empty;
                var result = false;
                switch (genType)
                {
                    case EnumGenType.Async:
                    case EnumGenType.Origin:
                        result = GenExecutor.Execute(sorceFile, genType, out errorMsg, out resultDic);
                        break;
                    case EnumGenType.AsyncDll:
                        result = GenExecutor.ExecuteDll(sorceFile, out errorMsg, out resultDic);
                        break;
                    case EnumGenType.AsyncClientDll:
                        result = GenExecutor.ExecuteClientDll(sorceFile, serviceModel, out errorMsg, out resultDic);
                        break;
                    case EnumGenType.AsyncClientNuget:
                        result = GenExecutor.ExcuteClientNuget(sorceFile, serviceModel, out errorMsg, out resultDic);
                        break;
                    default:
                        break;
                }

                if (result)
                {
                    // 打开当前目录
                    Process.Start(resultDic);
                }
                return result;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        private void DeleteDirAndFiles(string dir)
        {
            DirectoryInfo di = new DirectoryInfo(dir);
            if (di.GetDirectories().Length == 0 && di.GetFiles().Length == 0)
                return;
            foreach (DirectoryInfo d in di.GetDirectories())
                DeleteDirAndFiles(d.FullName);
            foreach (FileInfo fi in di.GetFiles())
            {
                fi.Delete();
            }

            di.Delete(true);
        }

        //#region 代码生成
        ///// <summary>
        ///// 打开代码生成目录
        ///// </summary>
        //void OpenGenCsharpDir()
        //{
        //    try
        //    {
        //        var codeDir = _GenCsharpDic;
        //        string[] directories = Directory.GetDirectories(codeDir);
        //        while (directories.Length > 0)
        //        {
        //            codeDir = directories[0];
        //            System.Threading.Thread.Sleep(100);
        //            directories = Directory.GetDirectories(codeDir);
        //        }
        //        System.Threading.Thread.Sleep(500);
        //        System.Diagnostics.Process.Start(codeDir);
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //}

        ///// <summary>
        ///// 执行
        ///// </summary>
        ///// <param name="fileName"></param>
        ///// <param name="inputList"></param>
        ///// <param name="output"></param>
        ///// <param name="error"></param>
        ///// <param name="workingDirectory"></param>
        ///// <param name="arguments"></param>
        //void InvokeProcess(string fileName, List<string> inputList, out string output, out string error, string workingDirectory = "", string arguments = "")
        //{
        //    System.Diagnostics.Process process = new System.Diagnostics.Process();
        //    process.StartInfo.FileName = fileName;
        //    if (!string.IsNullOrEmpty(workingDirectory))
        //    {
        //        process.StartInfo.WorkingDirectory = workingDirectory;
        //    }
        //    process.StartInfo.Arguments = arguments;
        //    process.StartInfo.UseShellExecute = false;
        //    process.StartInfo.RedirectStandardInput = true;
        //    process.StartInfo.RedirectStandardOutput = true;
        //    process.StartInfo.RedirectStandardError = true;
        //    process.StartInfo.CreateNoWindow = true;
        //    try
        //    {
        //        process.Start();
        //    }
        //    catch
        //    {
        //    }
        //    if (inputList != null && inputList.Count > 0)
        //    {
        //        foreach (string current in inputList)
        //        {
        //            process.StandardInput.WriteLine(current);
        //        }
        //    }
        //    output = process.StandardOutput.ReadToEnd();
        //    error = process.StandardError.ReadToEnd();
        //    process.WaitForExit();
        //    process.Close();
        //}

        //#endregion

    }
}
