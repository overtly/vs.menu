using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS.Menu.GrpcGenCore
{
    public enum EnumGrpcGenType
    {
        /// <summary>
        /// Origin
        /// </summary>
        Origin,
        /// <summary>
        /// Client
        /// </summary>
        Client,
        /// <summary>
        /// 服务端dll
        /// </summary>
        GenDll,
        /// <summary>
        /// 客户端Nuget包
        /// </summary>
        GenNuget,
    }
}
