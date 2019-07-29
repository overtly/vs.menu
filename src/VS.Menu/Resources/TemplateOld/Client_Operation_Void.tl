            
			public Task <%=methodName%>(<%=strParams%>object asyncState = null)
            {
                var taskSource_ = new TaskCompletionSource<bool>(asyncState);

                //构建请求发送buffer
                int seqID_ = this.seqGen_.NextSeqID();
                var sendBuffer_ = ThriftMarshaller.Serialize(new TMessage("<%=methodName%>", TMessageType.Call, seqID_),
                    new <%=declaringClassName%><%=methodName%>_args()
                    {
                        <%=setSendArgs%>
                    });

                //开始异步发送
                this.request_.Send(<%=hashingParams%>seqID_, sendBuffer_, (ex_) =>
                {
                    //处理异常回调
                    taskSource_.SetException(ex_);
                },
                (payload_) =>
                {
                    if (payload_ == null || payload_.Length == 0)
                    {
                        taskSource_.SetException(new TApplicationException(
                            TApplicationException.ExceptionType.MissingResult, "<%=methodName%> failed: Did not receive any data."));
                        return;
                    }

                    TMessage recvMsg_;
                    TApplicationException exServer_ = null;
                    <%=declaringClassName%><%=methodName%>_result result_ = null;

                    var oproto_ = ThriftMarshaller.GetBinaryProtocol(payload_);
                    try
                    {
                        //read TMessage
                        recvMsg_ = oproto_.ReadMessageBegin();
                        //read server return exception
                        if (recvMsg_.Type == TMessageType.Exception)
                            exServer_ = TApplicationException.Read(oproto_);
                        else
                        {
                            //read result
                            result_ = new <%=declaringClassName%><%=methodName%>_result();
                            result_.Read(oproto_);
                        }
                    }
                    catch (System.Exception ex_)
                    {
                        oproto_.Transport.Close();
                        taskSource_.SetException(ex_);
                        return;
                    }
                    oproto_.Transport.Close();

                    if (exServer_ != null)
					{
                        taskSource_.SetException(exServer_);
						return;
					}
					<%=ex%>
                    taskSource_.SetResult(true);
                });

                return taskSource_.Task;
            }