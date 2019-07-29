using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VS.Menu.ThriftGenCore
{
    public enum EnumGenType
    {
        Async,
        Origin,
        /// <summary>
        /// 服务端dll
        /// </summary>
        AsyncDll,
        /// <summary>
        /// 客户端Dll
        /// </summary>
        AsyncClientDll,
        /// <summary>
        /// 客户端Nuget包
        /// </summary>
        AsyncClientNuget,
    }
}
