        /// <summary>
        /// 异步客户端实现
        /// </summary>
        public class Client : Iface_client
        {
            #region Private Members
            /// <summary>
            /// seqID生成器
            /// </summary>
            private SeqIDGenerator seqGen_ = new SeqIDGenerator();
            /// <summary>
            /// thrift request
            /// </summary>
            private IAsyncThriftRequest request_ = null;
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="request"></param>
            /// <exception cref="ArgumentNullException">request is null</exception>
            public Client(IAsyncThriftRequest request)
            {
                if (request == null)
                    throw new ArgumentNullException("request");

                this.request_ = request;
            }
            #endregion

            #region Iface_client Members
            <%=Operations%>
            #endregion
        }