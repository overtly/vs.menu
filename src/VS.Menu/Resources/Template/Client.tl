        /// <summary>
        /// 异步客户端实现
        /// </summary>
        public class Client : Iface_client
        {
            #region Private Members
            /// <summary>
            /// thrift client
            /// </summary>
            private IThriftClient client_ = null;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="client"></param>
            /// <exception cref="ArgumentNullException">client is null</exception>
            public Client(IThriftClient client)
            {
                if (client == null)
                    throw new ArgumentNullException("client");

                this.client_ = client;
            }
            #endregion

            #region Iface_client Members
            <%=Operations%>
            #endregion
        }