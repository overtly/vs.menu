using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thrift;
using Thrift.Protocol;
using Thrift.Util;

namespace <%=namespace%>
{
    /// <summary>
    /// 异步<%=serviceName%>
    /// </summary>
    public class Async<%=serviceName%>
    {
		<%=bodyCode%>
    }
}