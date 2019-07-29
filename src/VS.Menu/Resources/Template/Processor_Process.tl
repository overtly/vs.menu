            
			private void <%=methodName%>_Process(TMessage message, TProtocol iproto, Action<byte[]> callback)
            {
                var args = new <%=declaringClassName%><%=methodName%>_args();
                try
                {
                    args.Read(iproto);
                }
                catch (System.Exception ex)
                {
                    iproto.Transport.Close();
                    callback(ThriftMarshaller.Serialize(new TMessage(message.Name, TMessageType.Exception, message.SeqID),
                        new TApplicationException(TApplicationException.ExceptionType.Unknown, ex.Message)));
                    return;
                }
                iproto.Transport.Close();

                int seqID = message.SeqID;
                try
                {
                    this._face.<%=methodName%>(<%=strParams%>(result) =>
                    {
                        callback(ThriftMarshaller.Serialize(new TMessage("<%=methodName%>", TMessageType.Reply, seqID),
                            new <%=declaringClassName%><%=methodName%>_result
                            {
                                Success = result
                            }));
                    });
                }
                catch (System.Exception ex)
                {
                    <%=ex%>callback(ThriftMarshaller.Serialize(new TMessage(message.Name, TMessageType.Exception, message.SeqID),
                        new TApplicationException(TApplicationException.ExceptionType.Unknown, ex.ToString())));
                }
            }