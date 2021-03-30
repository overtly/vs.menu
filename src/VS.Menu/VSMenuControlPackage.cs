using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using VS.Menu.GrpcGenCore;
using VS.Menu.Helper;
using VS.Menu.ThriftGenCore;

namespace VS.Menu
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidVSMenuControlPkgString)]
    [ProvideAutoLoad(UIContextGuids.NoSolution)]
    public sealed class VSMenuControlPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public VSMenuControlPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                #region Thrift
                #region Code Gen
                //Gen异步
                CommandID menuCommandID = new CommandID(GuidList.guidVSMenuContolCmdSet4Thrift, (int)PkgCmdIDList.ThriftGen4Async);
                OleMenuCommand menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
                menuItem.BeforeQueryStatus += menuItem4Thrift_BeforeQueryStatus;
                mcs.AddCommand(menuItem);

                //Gen异步Old
                CommandID menuCommandID_AsyncOld = new CommandID(GuidList.guidVSMenuContolCmdSet4Thrift, (int)PkgCmdIDList.ThriftGen4AsyncOld);
                OleMenuCommand menuItem_AsyncOld = new OleMenuCommand(MenuItem4AsyncOldCallback, menuCommandID_AsyncOld);
                menuItem_AsyncOld.BeforeQueryStatus += menuItem4Thrift_BeforeQueryStatus;
                mcs.AddCommand(menuItem_AsyncOld);

                //Gen 原生
                CommandID menuCommandID_Origin = new CommandID(GuidList.guidVSMenuContolCmdSet4Thrift, (int)PkgCmdIDList.ThriftGen4Origin);
                OleMenuCommand menuItem_Origin = new OleMenuCommand(MenuItem4OriginCallback, menuCommandID_Origin);
                menuItem_Origin.BeforeQueryStatus += menuItem4Thrift_BeforeQueryStatus;
                mcs.AddCommand(menuItem_Origin);
                #endregion

                #region Server DLL
                //Gen 异步客户端 Dll
                CommandID menuCommandID_AsyncDll = new CommandID(GuidList.guidVSMenuContolCmdSet4Thrift, (int)PkgCmdIDList.ThriftGen4AsyncDll);
                OleMenuCommand menuItem_AsyncDll = new OleMenuCommand(MenuItem4AsyncDllCallback, menuCommandID_AsyncDll);
                menuItem_AsyncDll.BeforeQueryStatus += menuItem4Thrift_BeforeQueryStatus;
                mcs.AddCommand(menuItem_AsyncDll);
                #endregion

                #region Client DLL
                //Gen 异步客户端 Dll
                CommandID menuCommandID_AsyncClientDll = new CommandID(GuidList.guidVSMenuContolCmdSet4Thrift, (int)PkgCmdIDList.ThriftGen4AsyncClientDll);
                OleMenuCommand menuItem_AsyncClientDll = new OleMenuCommand(MenuItem4AsyncClientDllCallback, menuCommandID_AsyncClientDll);
                menuItem_AsyncClientDll.BeforeQueryStatus += menuItem4Thrift_BeforeQueryStatus;
                mcs.AddCommand(menuItem_AsyncClientDll);

                //Gen 异步客户端Old Dll
                CommandID menuCommandID_AsyncOldClientDll = new CommandID(GuidList.guidVSMenuContolCmdSet4Thrift, (int)PkgCmdIDList.ThriftGen4AsyncOldClientDll);
                OleMenuCommand menuItem_AsyncOldClientDll = new OleMenuCommand(MenuItem4AsyncOldClientDllCallback, menuCommandID_AsyncOldClientDll);
                menuItem_AsyncOldClientDll.BeforeQueryStatus += menuItem4Thrift_BeforeQueryStatus;
                mcs.AddCommand(menuItem_AsyncOldClientDll);
                #endregion

                #region Client Nuget
                //Gen 异步客户端 Nuget
                CommandID menuCommandID_AsyncClientNuget = new CommandID(GuidList.guidVSMenuContolCmdSet4Thrift, (int)PkgCmdIDList.ThriftGen4AsyncClientNuget);
                OleMenuCommand menuItem_AsyncClientNuget = new OleMenuCommand(MenuItem4AsyncClientNugetCallback, menuCommandID_AsyncClientNuget);
                menuItem_AsyncClientNuget.BeforeQueryStatus += menuItem4Thrift_BeforeQueryStatus;
                mcs.AddCommand(menuItem_AsyncClientNuget);

                //Gen 异步客户端Old Nuget
                CommandID menuCommandID_AsyncOldClientNuget = new CommandID(GuidList.guidVSMenuContolCmdSet4Thrift, (int)PkgCmdIDList.ThriftGen4AsyncOldClientNuget);
                OleMenuCommand menuItem_AsyncOldClientNuget = new OleMenuCommand(MenuItem4AsyncOldClientNugetCallback, menuCommandID_AsyncOldClientNuget);
                menuItem_AsyncOldClientNuget.BeforeQueryStatus += menuItem4Thrift_BeforeQueryStatus;
                mcs.AddCommand(menuItem_AsyncOldClientNuget);
                #endregion

                #endregion

                #region Grpc
                #region Code Gen
                //GrpcServer Code
                CommandID menuCommandID_GrpcServer = new CommandID(GuidList.guidVSMenuControlCmdSet4Grpc, (int)PkgCmdIDList.GrpcGen4Server);
                OleMenuCommand menuItem_GrpcServer = new OleMenuCommand(MenuItem4GrpcServerCallback, menuCommandID_GrpcServer);
                menuItem_GrpcServer.BeforeQueryStatus += menuItem4Grpc_BeforeQueryStatus;
                mcs.AddCommand(menuItem_GrpcServer);

                // GrpcClient Code
                CommandID menuCommandID_GrpcClient = new CommandID(GuidList.guidVSMenuControlCmdSet4Grpc, (int)PkgCmdIDList.GrpcGen4Client);
                OleMenuCommand menuItem_GrpcClient = new OleMenuCommand(MenuItem4GrpcClientCallback, menuCommandID_GrpcClient);
                menuItem_GrpcClient.BeforeQueryStatus += menuItem4Grpc_BeforeQueryStatus;
                mcs.AddCommand(menuItem_GrpcClient);
                #endregion

                #region Dll Gen
                //GrpcServer Dll
                CommandID menuCommandID_GrpcServerDll = new CommandID(GuidList.guidVSMenuControlCmdSet4Grpc, (int)PkgCmdIDList.GrpcGen4ClientDll);
                OleMenuCommand menuItem_GrpcServerDll = new OleMenuCommand(MenuItem4GrpcDllCallback, menuCommandID_GrpcServerDll);
                menuItem_GrpcServerDll.BeforeQueryStatus += menuItem4Grpc_BeforeQueryStatus;
                mcs.AddCommand(menuItem_GrpcServerDll);
                #endregion

                #region Nuget Gen
                //GrpcServer Nuget
                CommandID menuCommandID_GrpcServerNuget = new CommandID(GuidList.guidVSMenuControlCmdSet4Grpc, (int)PkgCmdIDList.GrpcGen4ClientNuget);
                OleMenuCommand menuItem_GrpcServerNuget = new OleMenuCommand(MenuItem4GrpcNugetCallback, menuCommandID_GrpcServerNuget);
                menuItem_GrpcServerNuget.BeforeQueryStatus += menuItem4Grpc_BeforeQueryStatus;
                mcs.AddCommand(menuItem_GrpcServerNuget);
                #endregion

                #region Nuget Gen (Grpc Net)
                //GrpcServer Nuget
                CommandID menuCommandID_GrpcServerNuget_GrpcNet = new CommandID(GuidList.guidVSMenuControlCmdSet4Grpc, (int)PkgCmdIDList.GrpcGen4ClientNuget_GrpcNet);
                OleMenuCommand menuItem_GrpcServerNuget_GrpcNet = new OleMenuCommand(MenuItem4GrpcNuget_GrpcNetCallback, menuCommandID_GrpcServerNuget_GrpcNet);
                menuItem_GrpcServerNuget.BeforeQueryStatus += menuItem4Grpc_BeforeQueryStatus;
                mcs.AddCommand(menuItem_GrpcServerNuget_GrpcNet);
                #endregion
                #endregion
            }
        }
        #endregion

        #region 右键点击
        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void menuItem4Thrift_BeforeQueryStatus(object sender, EventArgs e)
        {
            if (!(sender is OleMenuCommand menuCommand))
                return;
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            var fileName = dte.ActiveDocument.FullName;
            var isThriftFile = !string.IsNullOrEmpty(fileName) && fileName.ToLower().EndsWith(".thrift");

            if (!isThriftFile)
            {
                menuCommand.Visible = false;
                return;
            }
            menuCommand.Visible = true;
        }

        private void menuItem4Grpc_BeforeQueryStatus(object sender, EventArgs e)
        {
            if (!(sender is OleMenuCommand menuCommand))
                return;
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            var fileName = dte.ActiveDocument.FullName;
            var isGrpcFile = !string.IsNullOrEmpty(fileName) && fileName.ToLower().EndsWith(".proto");
            if (!isGrpcFile)
            {
                menuCommand.Visible = false;
                return;
            }
            menuCommand.Visible = true;
        }
        #endregion


        #region ThriftMethod
        #region Gen Code
        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            if (Utility.InProcess)
            {
                ShowMsg("上一个命令正在执行,请等待...");
                return;
            }
            ShowProcess(dte);

            var errorMsg = string.Empty;
            var result = ThriftGenerator.Default.GenCsharp(dte.ActiveDocument.FullName, EnumGenType.Async, out errorMsg);
            if (!result)
                ShowMsg(errorMsg);

            HideProcess(dte);
        }

        private void MenuItem4AsyncOldCallback(object sender, EventArgs e)
        {
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            if (Utility.InProcess)
            {
                ShowMsg("上一个命令正在执行,请等待...");
                return;
            }
            ShowProcess(dte, EnumGenAsyncVersion.Old);

            var errorMsg = string.Empty;
            var result = ThriftGenerator.Default.GenCsharp(dte.ActiveDocument.FullName, EnumGenType.Async, out errorMsg);
            if (!result)
                ShowMsg(errorMsg);

            HideProcess(dte);
        }

        private void MenuItem4OriginCallback(object sender, EventArgs e)
        {
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            if (Utility.InProcess)
            {
                ShowMsg("上一个命令正在执行,请等待...");
                return;
            }
            ShowProcess(dte);

            var errorMsg = string.Empty;
            var result = ThriftGenerator.Default.GenCsharp(dte.ActiveDocument.FullName, EnumGenType.Origin, out errorMsg);
            if (!result)
                ShowMsg(errorMsg);

            HideProcess(dte);
        }

        #endregion

        #region Gen Server Dll
        /// <summary>
        /// Gen Server Dll
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem4AsyncDllCallback(object sender, EventArgs e)
        {
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            if (Utility.InProcess)
            {
                ShowMsg("上一个命令正在执行,请等待...");
                return;
            }
            ShowProcess(dte);

            var errorMsg = string.Empty;
            var result = ThriftGenerator.Default.GenCsharp(dte.ActiveDocument.FullName, EnumGenType.AsyncDll, out errorMsg);
            if (!result)
                ShowMsg(errorMsg);

            HideProcess(dte);
        }
        #endregion

        #region Gen Client Dll
        /// <summary>
        /// Gen Client Dll New
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem4AsyncClientDllCallback(object sender, EventArgs e)
        {
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            ThriftGlobal.GenAsyncVersion = EnumGenAsyncVersion.New;
            var newWindow = new ServiceOption4Thrift(dte, EnumGenType.AsyncClientDll);
            newWindow.ShowDialog();
        }

        /// <summary>
        /// Gen Client Dll Old
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem4AsyncOldClientDllCallback(object sender, EventArgs e)
        {
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            ThriftGlobal.GenAsyncVersion = EnumGenAsyncVersion.Old;
            var newWindow = new ServiceOption4Thrift(dte, EnumGenType.AsyncClientDll);
            newWindow.ShowDialog();
        }
        #endregion

        #region Gen Client Nuget
        /// <summary>
        /// Gen Client Nuget New
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem4AsyncClientNugetCallback(object sender, EventArgs e)
        {
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            ThriftGlobal.GenAsyncVersion = EnumGenAsyncVersion.New;
            var newWindow = new ServiceOption4Thrift(dte, EnumGenType.AsyncClientNuget);
            newWindow.ShowDialog();
        }

        /// <summary>
        /// Gen Client Nuget Old
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem4AsyncOldClientNugetCallback(object sender, EventArgs e)
        {
            if (!(GetService(typeof(DTE)) is DTE dte))
                return;

            ThriftGlobal.GenAsyncVersion = EnumGenAsyncVersion.Old;
            var newWindow = new ServiceOption4Thrift(dte, EnumGenType.AsyncClientNuget);
            newWindow.ShowDialog();

        }
        #endregion
        #endregion

        #region GrpcMethod
        private void MenuItem4GrpcServerCallback(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (!CheckDte(dte))
                return;

            if (Utility.InProcess)
            {
                ShowMsg("上一个命令正在执行,请等待...");
                return;
            }
            ShowProcess(dte);

            var errorMsg = string.Empty;
            var result = GrpcGenerator.Default.GenCsharp(dte.ActiveDocument.FullName, EnumGrpcGenType.Origin, out errorMsg);
            if (!result)
                ShowMsg(errorMsg);

            HideProcess(dte);
        }

        private void MenuItem4GrpcClientCallback(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (!CheckDte(dte))
                return;

            if (Utility.InProcess)
            {
                ShowMsg("上一个命令正在执行,请等待...");
                return;
            }
            ShowProcess(dte);

            var errorMsg = string.Empty;
            var result = GrpcGenerator.Default.GenCsharp(dte.ActiveDocument.FullName, EnumGrpcGenType.Client, out errorMsg);
            if (!result)
                ShowMsg(errorMsg);

            HideProcess(dte);
        }

        private void MenuItem4GrpcDllCallback(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (!CheckDte(dte))
                return;

            if (Utility.InProcess)
            {
                ShowMsg("上一个命令正在执行,请等待...");
                return;
            }
            ShowProcess(dte);

            var errorMsg = string.Empty;
            var result = GrpcGenerator.Default.GenCsharp(dte.ActiveDocument.FullName, EnumGrpcGenType.GenDll, out errorMsg);
            if (!result)
                ShowMsg(errorMsg);

            HideProcess(dte);
        }

        private void MenuItem4GrpcNugetCallback(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (!CheckDte(dte))
                return;

            var newWindow = new ServiceOption4Grpc(dte);
            newWindow.ShowDialog();
        }

        private void MenuItem4GrpcNuget_GrpcNetCallback(object sender, EventArgs e)
        {
            var dte = GetService(typeof(DTE)) as DTE;
            if (!CheckDte(dte))
                return;

            var newWindow = new ServiceOption4Grpc_GrpcNet(dte);
            newWindow.ShowDialog();
        }
        #endregion


        #region Custom Method
        private void ShowMsg(string msg)
        {
            if (string.IsNullOrEmpty(msg))
                return;

            var uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            var clsid = Guid.Empty;
            int result;

            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "VSMenu提示",
                       msg,
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }

        private void ShowProcess(DTE dte, EnumGenAsyncVersion genAsyncVersion = EnumGenAsyncVersion.New)
        {
            if (dte != null)
            {
                dte.StatusBar.Progress(true, "正在生成中,请稍候...", 0, 100);
            }

            Utility.InProcess = true;
            ThriftGlobal.GenAsyncVersion = genAsyncVersion;
        }

        private void HideProcess(DTE dte)
        {
            if (dte != null)
            {
                //dte.StatusBar.Clear();
                dte.StatusBar.Progress(false);
            }

            Utility.InProcess = false;
        }

        private bool CheckDte(DTE dte)
        {
            if (dte == null)
                return false;

            var filePath = dte.ActiveDocument.FullName;
            if (Regex.IsMatch(filePath, @"[\u4e00-\u9fbb]"))
            {
                ShowMsg("模板文件不能包含在中文文件夹中...");
                return false;
            }
            return true;
        }
        #endregion

    }
}
