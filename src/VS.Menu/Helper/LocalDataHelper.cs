using System;
using System.IO;
using VS.Menu.Model;

namespace VS.Menu.Helper
{
    /// <summary>
    /// 保存在客户端本地C盘下的配置文件
    /// 方便用户不用每次更新后都要重新设置
    /// </summary>
    public class LocalDataHelper
    {
        //private static readonly string filePath = Path.Combine(Global.AppBaseDic, "thrift4GenServiceData.xml");

        /// <summary>
        /// 保存thrift模板文件的文件夹路径
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static void Save(ServiceModel model, string thriftFilePath)
        {
            if (model == null || string.IsNullOrEmpty(thriftFilePath))
                return;
            if (!File.Exists(thriftFilePath))
                return;

            var filePath = thriftFilePath + ".xml";
            var thriftFile = new FileInfo(thriftFilePath);
            var data = new LocalData()
            {
                ThriftFileName = thriftFile.Name,
                ServiceName = model.ServiceName,
                ConfigServiceName = model.ConfigServiceName,
                NugetId = model.NugetId,
                Port = model.Port,
                Publish = model.Publish,
            };
            XmlHelper.XmlSerializeToFile(data, filePath);



            //var list = new List<LocalData>();
            //try
            //{
            //    if (File.Exists(filePath))
            //        list = XmlHelper.XmlDeserializeFromFile<List<LocalData>>(filePath) ?? new List<LocalData>();
            //}
            //catch (Exception) { }

            //var existData = list.Find(oo => oo.ThriftFileName == thriftFileName);
            //if (existData != null && existData.ThriftFileName == thriftFileName)
            //    list.Remove(existData);

            //var data = new LocalData()
            //{
            //    ThriftFileName = thriftFileName,
            //    ServiceName = model.ServiceName,
            //    ConfigServiceName = model.ConfigServiceName,
            //    NugetId = model.NugetId,
            //    Port = model.Port,
            //    Publish = model.Publish,
            //};
            //list.Add(data);
            //XmlHelper.XmlSerializeToFile(list, filePath);
        }

        /// <summary>
        /// 获取
        /// </summary>
        /// <param name="thriftFileName"></param>
        /// <returns></returns>
        public static ServiceModel Get(string thriftFilePath)
        {
            if (string.IsNullOrEmpty(thriftFilePath))
                return new ServiceModel();
            if (!File.Exists(thriftFilePath))
                return new ServiceModel();

            var filePath = thriftFilePath + ".xml";
            var data = new LocalData();
            try
            {
                if (File.Exists(filePath))
                    data = XmlHelper.XmlDeserializeFromFile<LocalData>(filePath) ?? new LocalData();
            }
            catch (Exception) { }
            if (data != null && !string.IsNullOrEmpty(data.ThriftFileName))
                return new ServiceModel()
                {
                    ServiceName = data.ServiceName,
                    ConfigServiceName = string.IsNullOrEmpty(data.ConfigServiceName) ? data.ServiceName : data.ConfigServiceName,
                    NugetId = data.NugetId,
                    Port = data.Port,
                    Publish = data.Publish
                };

            return new ServiceModel();


            //var list = new List<LocalData>();
            //try
            //{
            //    if (File.Exists(filePath))
            //        list = XmlHelper.XmlDeserializeFromFile<List<LocalData>>(filePath) ?? new List<LocalData>();
            //}
            //catch (Exception) { }

            //var existData = list.Find(oo => oo.ThriftFileName == thriftFileName);
            //if (existData != null && existData.ThriftFileName == thriftFileName)
            //    return new ServiceModel()
            //    {
            //        ServiceName = existData.ServiceName,
            //        ConfigServiceName = string.IsNullOrEmpty(existData.ConfigServiceName) ? existData.ServiceName : existData.ConfigServiceName,
            //        NugetId = existData.NugetId,
            //        Port = existData.Port,
            //        Publish = existData.Publish
            //    };

            //return new ServiceModel();
        }
    }


    public class LocalData
    {
        public string ThriftFileName { get; set; }
        public string ServiceName { get; set; }
        public string ConfigServiceName { get; set; }
        public string NugetId { get; set; }
        public int Port { get; set; }
        public bool Publish { get; set; }
    }
}