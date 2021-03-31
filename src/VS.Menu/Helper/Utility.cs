using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace VS.Menu.Helper
{
    public static class Utility
    {
        #region Static Property
        /// <summary>
        /// 是否正在进行中
        /// </summary>
        public static bool InProcess = false;
        /// <summary>
        /// 
        /// </summary>
        public static string TmpFoldName = "TmpVSMenu";
        /// <summary>
        /// 应用域所在的路径
        /// </summary>
        public static string AppBaseDic = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 资源位置
        /// </summary>
        public static string AppBaseResourceDic
        {
            get
            {
                var dic = Path.Combine(AppBaseDic, "Resource");
                if (!Directory.Exists(dic))
                    Directory.CreateDirectory(dic);
                return dic;
            }
        }

        #region Nuget.exe
        /// <summary>
        /// FastSocketClient
        /// </summary>
        public static string NugetExePath
        {
            get
            {
                var path = Path.Combine(Utility.AppBaseResourceDic, "Nuget.exe");
                ExportFile(path, Resources.NuGetExe);

                return path;
            }
        }
        #endregion

        #endregion
        static Utility()
        {
            #region 创建临时目录
            var rootPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrEmpty(rootPath))
                rootPath = $"C:\\Documents";

            AppBaseDic = Path.Combine(rootPath, TmpFoldName);
            if (!Directory.Exists(AppBaseDic))
            {
                try { Directory.CreateDirectory(AppBaseDic); }
                catch { }
            }
            #endregion
        }

        #region Public Methods
        /// <summary>
        /// 检测文件夹目录存在
        /// </summary>
        /// <param name="dicPath"></param>
        /// <returns></returns>
        public static bool CheckDicPath(string dicPath)
        {
            return !string.IsNullOrEmpty(dicPath) && Directory.Exists(dicPath);
        }

        /// <summary>
        /// 检测文件夹的存在
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool CheckFilePath(string filePath)
        {
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }


        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="dicPath"></param>
        public static void MakeDir(string dicPath)
        {
            if (Directory.Exists(dicPath))
                return;
            var info = Directory.CreateDirectory(dicPath);
            Thread.Sleep(20);
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="dirPath"></param>
        public static void DelDir(string dirPath)
        {
            try
            {
                Directory.Delete(dirPath, true);
            }
            catch { }
        }

        /// <summary>
        /// 清空文件夹，不存在则创建
        /// </summary>
        /// <param name="dirPath"></param>
        public static void ClearDir(string dirPath)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    MakeDir(dirPath);
                    return;
                }

                var dirInfo = new DirectoryInfo(dirPath);
                dirInfo.Delete(true);
                System.Threading.Thread.Sleep(20);
                MakeDir(dirPath);
            }
            catch { }
        }

        /// <summary>
        /// 删除前缀目录
        /// </summary>
        /// <param name="dicPath"></param>
        public static void DelPrefixDir(string dirPrefixPath)
        {
            try
            {
                var dirs = Directory.GetDirectories(AppBaseDic);
                foreach (var item in dirs)
                {
                    if (item.StartsWith(dirPrefixPath))
                        Directory.Delete(item, true);
                }
            }
            catch { }
        }

        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="sourceFolderName">源文件夹目录</param>
        /// <param name="destFolderName">目标文件夹目录</param>
        public static void Copy(string sourceFolderName, string destFolderName)
        {
            Copy(sourceFolderName, destFolderName, false);
        }

        /// <summary>
        /// 复制文件夹
        /// </summary>
        /// <param name="sourceFolderName">源文件夹目录</param>
        /// <param name="destFolderName">目标文件夹目录</param>
        /// <param name="overwrite">允许覆盖文件</param>
        public static void Copy(string sourceFolderName, string destFolderName, bool overwrite)
        {
            var sourceFilesPath = Directory.GetFileSystemEntries(sourceFolderName);

            for (int i = 0; i < sourceFilesPath.Length; i++)
            {
                var sourceFilePath = sourceFilesPath[i];
                var directoryName = Path.GetDirectoryName(sourceFilePath);
                var forlders = directoryName.Split('\\');
                var lastDirectory = forlders[forlders.Length - 1];
                var dest = Path.Combine(destFolderName, lastDirectory);

                if (File.Exists(sourceFilePath))
                {
                    var sourceFileName = Path.GetFileName(sourceFilePath);
                    if (!Directory.Exists(dest))
                    {
                        Directory.CreateDirectory(dest);
                    }
                    File.Copy(sourceFilePath, Path.Combine(dest, sourceFileName), overwrite);
                }
                else
                {
                    Copy(sourceFilePath, dest, overwrite);
                }
            }
        }

        /// <summary>
        /// 导出文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="resource"></param>
        public static void ExportFile(string filePath, byte[] resource)
        {
            #region 导出Thrift.dll
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                byte[] buffer = resource;//这个是添加EXE到程序资源时的名称
                FileStream fileStream = null;
                BinaryWriter bWriter = null;
                try
                {
                    fileStream = new FileStream(filePath, FileMode.Create);//新建文件
                    bWriter = new BinaryWriter(fileStream);//以二进制打开文件流
                    bWriter.Write(buffer, 0, buffer.Length);//从资源文件读取文件内容，写入到一个文件中
                }
                catch { }
                finally
                {
                    if (bWriter != null)
                    {
                        bWriter.Close();
                    }
                    if (fileStream != null)
                        fileStream.Close();
                }
            }
            #endregion
        }
        #endregion

        #region Public Method
        /// <summary>
        /// 删除资源文件夹
        /// </summary>
        public static void DeleteResource()
        {
            DelDir(AppBaseResourceDic);
        }
        /// <summary>
        /// 创建or覆盖得到一个新文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public static void WriteNewFile(string fileName, string content)
        {
            byte[] str = Encoding.UTF8.GetBytes(content);
            FileStream fs = null;
            try
            {
                fs = File.Create(fileName);
                fs.Write(str, 0, str.Length);
                fs.Flush();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("create file error,filename:" + fileName + Environment.NewLine + e.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            finally
            {
                if (fs != null)
                    fs.Close();
            }
        }

        /// <summary>
        /// 静默执行一个程序
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="arguments"></param>
        public static void RunProcess(string fileName, string workingDirectory = "", string arguments = "", bool useShellExecute = true)
        {
            ProcessStartInfo pInfo = new ProcessStartInfo();
            pInfo.FileName = fileName;
            if (!string.IsNullOrEmpty(workingDirectory))
                pInfo.WorkingDirectory = workingDirectory;
            pInfo.Arguments = arguments;

            pInfo.Verb = "runas";
            pInfo.UseShellExecute = useShellExecute;
            pInfo.CreateNoWindow = false;
            pInfo.WindowStyle = ProcessWindowStyle.Hidden;
            try
            {
                Process p = Process.Start(pInfo);
                p.WaitForExit();
            }
            catch { }
        }

        /// <summary>
        /// 执行CMD
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        public static bool RunCmd(string fileName, string arguments, out string errorMsg)
        {
            errorMsg = string.Empty;
            return RunCmd(fileName, arguments, "", out errorMsg);
        }

        /// <summary>
        /// 执行CMD
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="arguments"></param>
        public static bool RunCmd(string fileName, string arguments, string workDirectory, out string errorMsg)
        {
            errorMsg = string.Empty;
            var str = string.Format(@"""{0}"" {1} {2}", fileName, arguments, "&exit");
            try
            {
                using (Process myPro = new Process())
                {
                    myPro.StartInfo.FileName = "cmd.exe";
                    myPro.StartInfo.UseShellExecute = false;
                    myPro.StartInfo.RedirectStandardInput = true;
                    myPro.StartInfo.RedirectStandardOutput = true;
                    myPro.StartInfo.RedirectStandardError = true;
                    myPro.StartInfo.CreateNoWindow = false;
                    if (!string.IsNullOrEmpty(workDirectory) && Directory.Exists(workDirectory))
                        myPro.StartInfo.WorkingDirectory = workDirectory;
                    myPro.Start();

                    myPro.StandardInput.WriteLine(str);
                    myPro.StandardInput.AutoFlush = true;
                    errorMsg = myPro.StandardOutput.ReadToEnd();

                    myPro.WaitForExit();
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 需要输出的执行一个程序
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="inputList"></param>
        /// <param name="output"></param>
        /// <param name="error"></param>
        /// <param name="workingDirectory"></param>
        /// <param name="arguments"></param>
        public static void InvokeProcess(string fileName, List<string> inputList, out string output,
            out string error, string workingDirectory = "", string arguments = "")
        {
            var pInfo = new Process();
            pInfo.StartInfo.FileName = fileName;
            if (!string.IsNullOrEmpty(workingDirectory))
                pInfo.StartInfo.WorkingDirectory = workingDirectory;
            pInfo.StartInfo.Arguments = arguments;

            pInfo.StartInfo.Verb = "runas";
            pInfo.StartInfo.UseShellExecute = false;
            pInfo.StartInfo.RedirectStandardInput = true;
            pInfo.StartInfo.RedirectStandardOutput = true;
            pInfo.StartInfo.RedirectStandardError = true;
            pInfo.StartInfo.CreateNoWindow = true;

            try
            {
                pInfo.Start();
            }
            catch
            {

            }
            if (inputList != null && inputList.Count > 0)
            {
                foreach (var input in inputList)
                {
                    pInfo.StandardInput.WriteLine(input);
                }
            }
            output = pInfo.StandardOutput.ReadToEnd();
            error = pInfo.StandardError.ReadToEnd();
            pInfo.WaitForExit();
            pInfo.Close();
        }


        #endregion
    }
}
