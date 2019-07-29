using System;
using System.IO;
using System.Threading;
using VS.Menu.Model;
using VS.Menu.ThriftGenCore;

namespace VS.Menu.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public class NugetServerHelper
    {
        /// <summary>
        /// 获取
        /// </summary>
        /// <returns></returns>
        public static NugetServerModel Get()
        {
            var data = new NugetServerModel();
            var fileName = "setting.data";
            var address = "http://10.0.60.89:8081";
            if (ThriftGlobal.GenAsyncVersion == EnumGenAsyncVersion.Old)
            {
                fileName = $"setting_{ThriftGlobal.GenAsyncVersion}.data";
                address = "http://10.0.60.89:8080";
            }

            var filePath = Path.Combine(Utility.AppBaseDic, "nuget", fileName);
            if (string.IsNullOrEmpty(filePath))
                return data;

            try
            {
                if (File.Exists(filePath))
                    data = XmlHelper.XmlDeserializeFromFile<NugetServerModel>(filePath) ?? new NugetServerModel();
            }
            catch (Exception) { }

            if (string.IsNullOrEmpty(data?.Address))
            {
                data = new NugetServerModel()
                {
                    Address = address,
                    Key = "123456"
                };
                Save(data);
            }

            return data;
        }

        /// <summary>
        /// 保存thrift模板文件的文件夹路径
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static void Save(NugetServerModel data)
        {
            if (data == null || string.IsNullOrEmpty(data.Address))
                return;

            var fold = Path.Combine(Utility.AppBaseDic, "nuget");
            if (!Directory.Exists(fold))
                Directory.CreateDirectory(fold);

            var fileName = "setting.data";
            if (ThriftGlobal.GenAsyncVersion == EnumGenAsyncVersion.Old)
                fileName = $"setting_{ThriftGlobal.GenAsyncVersion}.data";

            var filePath = Path.Combine(fold, fileName);
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
                Thread.Sleep(20);
            }

            XmlHelper.XmlSerializeToFile(data, filePath);
        }

    }
}