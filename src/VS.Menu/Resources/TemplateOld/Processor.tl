        /// <summary>
        /// 异步Processor
        /// </summary>
        public class Processor : IAsyncProcessor
        {
            #region Delegates
            /// <summary>
            /// process handle delegate
            /// </summary>
            /// <param name="message"></param>
            /// <param name="iproto"></param>
            /// <param name="callback"></param>
            private delegate void ProcessHandle(TMessage message, TProtocol iproto, Action<byte[]> callback);
            #endregion

            #region Private Members
            /// <summary>
            /// service实现对象
            /// </summary>
            private Iface_server _face = null;
            /// <summary>
            /// process handle dic
            /// </summary>
            private Dictionary<string, ProcessHandle> processMap_ =
                 new Dictionary<string, ProcessHandle>();
            #endregion

            #region Constructors
            /// <summary>
            /// new
            /// </summary>
            /// <param name="face"></param>
            /// <exception cref="ArgumentNullException">face is null</exception>
            public Processor(Iface_server face)
            {
                if (face == null)
                    throw new ArgumentNullException("face");

                this._face = face;
				<%=ProcessMap%>
            }
            #endregion

            #region IAsyncProcessor Members
            /// <summary>
            /// process
            /// </summary>
            /// <param name="payload"></param>
            /// <param name="callback"></param>
            public void Process(byte[] payload, Action<byte[]> callback)
            {
                var iproto = ThriftMarshaller.GetBinaryProtocol(payload);

                TMessage message;
                try
                {
                    message = iproto.ReadMessageBegin();
                }
                catch (System.Exception)
                {
                    iproto.Transport.Close();
                    return;
                }

                ProcessHandle handle = null;
                if (this.processMap_.TryGetValue(message.Name, out handle))
                {
                    handle(message, iproto, callback);
                }
                else
                {
                    iproto.Transport.Close();
                    callback(ThriftMarshaller.Serialize(new TMessage(message.Name, TMessageType.Exception, message.SeqID),
                        new TApplicationException(TApplicationException.ExceptionType.UnknownMethod,
                            string.Concat("Invalid method name: '", message.Name, "'"))));
                }
            }
            #endregion

            #region Private Methods
			<%=ProcessMethods%>
            #endregion
        }