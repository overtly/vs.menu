using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS.Menu.Model
{
    public class ServiceModel
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; }
        /// <summary>
        /// 配置文件的名称
        /// 默认用ServiceName
        /// </summary>
        public string ConfigServiceName { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 直接发布
        /// </summary>
        public bool Publish { get; set; }
        /// <summary>
        /// NugetId
        /// </summary>
        public string NugetId { get; set; }
        /// <summary>
        /// 是否忽略警告
        /// </summary>
        public bool IsIgnore { get; set; }
    }
}
