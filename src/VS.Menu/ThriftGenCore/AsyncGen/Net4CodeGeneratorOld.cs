using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VS.Menu.ThriftGenCore.AsyncGen
{
    public class Net4CodeGeneratorOld : ICodeGenerator
    {
        //相关模板
        protected string TlClient = Encoding.UTF8.GetString(Resources.Client1);

        protected string TlClientOperation = Encoding.UTF8.GetString(Resources.Client_Operation1);

        protected string TlClientOperationVoid = Encoding.UTF8.GetString(Resources.Client_Operation_Void1);

        protected string TlFaceClient = Encoding.UTF8.GetString(Resources.Face_Client1);

        protected string TlFaceClientOperation = Encoding.UTF8.GetString(Resources.Face_Client_Operation1);

        protected string TlAsyncService = Encoding.UTF8.GetString(Resources.AsyncService1);

        protected string TlFaceServer = Encoding.UTF8.GetString(Resources.Face_Server1);

        protected string TlFaceServerOperation = Encoding.UTF8.GetString(Resources.Face_Server_Operation1);

        protected string TlProcessor = Encoding.UTF8.GetString(Resources.Processor1);

        protected string TlProcessorProcess = Encoding.UTF8.GetString(Resources.Processor_Process1);

        protected string TlProcessorProcessVoid = Encoding.UTF8.GetString(Resources.Processor_Process_Void1);

        #region ICodeGenerator Members
        /// <summary>
        /// generate
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public IList<CodeFullResult> Generate(ThriftTemplate template)
        {
            if (template == null)
                return null;

            string[] arrHashParams = new string[] { };

            List<CodeFullResult> list = new List<CodeFullResult>();
            foreach (var childService in template.Services)
            {
                var sb = new StringBuilder();

                #region 生成接口代码
                string strFaceOperations = string.Empty;
                foreach (var childOp in childService.Operations)
                {
                    var strParams = string.Join(", ", childOp.ListParam.ToArray());
                    if (!string.IsNullOrEmpty(strParams))
                        strParams = strParams + ", ";

                    strFaceOperations += TlFaceClientOperation
                        .Replace("<%=returnType%>", childOp.ReturnType.ToLower() == "system.void" ? string.Empty : "<" + childOp.ReturnType + ">")
                        .Replace("<%=methodName%>", childOp.MethodName)
                        .Replace("<%=params%>", strParams);
                    strFaceOperations += System.Environment.NewLine;
                }
                sb.Append(TlFaceClient.Replace("<%=Operations%>", strFaceOperations));
                sb.Append(Environment.NewLine);
                #endregion

                #region 生成client代码
                string strClientOperations = string.Empty;
                foreach (var childOp in childService.Operations)
                {
                    string ex = string.Empty;
                    //处理服务端异常
                    var resultType = childService.Assembly.GetTypes()
                        .Where(c => c.FullName.LastIndexOf(childOp.MethodName + "_result") > -1)
                        .FirstOrDefault();
                    if (resultType != null)
                    {
                        var exlist = resultType.GetProperties().Where(c =>
                        {
                            return c.PropertyType.IsSubclassOf(typeof(Exception));
                        }).ToList();
                        if (exlist != null && exlist.Count > 0)
                        {
                            var sbEx = new StringBuilder();
                            foreach (var child in exlist)
                            {
                                sbEx.AppendFormat("if (result_.__isset.{0}) {{", child.Name[0].ToString().ToLower() + child.Name.Remove(0, 1));
                                sbEx.Append(Environment.NewLine);
                                sbEx.AppendFormat("taskSource_.SetException(result_.{0});", child.Name);
                                sbEx.Append(Environment.NewLine);
                                sbEx.Append("return;");
                                sbEx.Append(Environment.NewLine);
                                sbEx.Append("}");
                                sbEx.Append(Environment.NewLine);
                            }
                            ex = sbEx.ToString();
                        }
                    }

                    var strParams = string.Join(", ", childOp.ListParam.ToArray());
                    if (!string.IsNullOrEmpty(strParams))
                        strParams = strParams + ", ";

                    string ophashParmas = string.Empty;
                    foreach (var childParam in childOp.ListParam)
                    {
                        var p = childParam.Split(' ')[1];
                        if (arrHashParams.Contains(p.ToLower()))
                        {
                            ophashParmas = "System.BitConverter.GetBytes(" + p + ")" + ", ";
                            break;
                        }
                    }

                    if (childOp.ReturnType.ToLower() == "system.void")
                    {
                        strClientOperations += TlClientOperationVoid.Replace("<%=methodName%>", childOp.MethodName)
                            .Replace("<%=strParams%>", strParams)
                            .Replace("<%=setSendArgs%>", GetSetArgsString(childOp.ListParam))
                            .Replace("<%=declaringClassName%>", string.IsNullOrEmpty(childService.DeclaringClassName) ? "" : childService.DeclaringClassName + ".")
                            .Replace("<%=ex%>", ex)
                            .Replace("<%=serviceName%>", childService.FullName)
                            .Replace("<%=hashingParams%>", ophashParmas);
                    }
                    else
                    {
                        strClientOperations += TlClientOperation.Replace("<%=methodName%>", childOp.MethodName)
                            .Replace("<%=strParams%>", strParams)
                            .Replace("<%=setSendArgs%>", GetSetArgsString(childOp.ListParam))
                            .Replace("<%=returnType%>", childOp.ReturnType)
                            .Replace("<%=declaringClassName%>", string.IsNullOrEmpty(childService.DeclaringClassName) ? "" : childService.DeclaringClassName + ".")
                            .Replace("<%=serviceName%>", childService.FullName)
                            .Replace("<%=ex%>", ex)
                            .Replace("<%=hashingParams%>", ophashParmas);
                    }
                }
                sb.Append(TlClient.Replace("<%=Operations%>", strClientOperations));
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                #endregion

                #region 生成face_server
                string strFaceServerOperations = string.Empty;
                foreach (var childOp in childService.Operations)
                {
                    var strParams = string.Join(", ", childOp.ListParam.ToArray());
                    if (!string.IsNullOrEmpty(strParams))
                        strParams = strParams + ", ";

                    strFaceServerOperations += this.TlFaceServerOperation
                        .Replace("<%=returnType%>", childOp.ReturnType.ToLower() == "system.void" ? string.Empty : "<" + childOp.ReturnType + ">")
                        .Replace("<%=methodName%>", childOp.MethodName)
                        .Replace("<%=params%>", strParams);

                    strFaceServerOperations += System.Environment.NewLine;
                }
                sb.Append(this.TlFaceServer.Replace("<%=Operations%>", strFaceServerOperations));
                sb.Append(Environment.NewLine);
                #endregion

                #region 生成processor.process
                string strProcessorProcess = string.Empty;
                foreach (var childOp in childService.Operations)
                {

                    string ex = string.Empty;
                    var resultType = childService.Assembly.GetTypes()
                        .Where(c => c.FullName.LastIndexOf(childOp.MethodName + "_result") > -1)
                        .FirstOrDefault();
                    if (resultType != null)
                    {
                        var exlist = resultType.GetProperties().Where(c =>
                        {
                            return c.PropertyType.IsSubclassOf(typeof(Exception));
                        }).ToList();
                        if (exlist != null && exlist.Count > 0)
                        {
                            var sbEx = new StringBuilder();
                            foreach (var child in exlist)
                            {
                                //try
                                //{

                                //}
                                //catch (System.Exception ex)
                                //{
                                //    if (ex is ArgumentException)
                                //    {

                                //callback(ThriftMarshaller.Serialize(new TMessage("<%=methodName%>", TMessageType.Reply, seqID),
                                //    new <%=declaringClassName%><%=methodName%>_result
                                //    {
                                //        Ex = ex as ArgumentException
                                //    }));
                                //return;
                                //}
                                //}
                                //sbEx.AppendFormat("if (result_.__isset.{0}) {{", child.Name[0].ToString().ToLower() + child.Name.Remove(0, 1));
                                //sbEx.Append(Environment.NewLine);
                                //sbEx.AppendFormat("taskSource_.SetException(result_.{0});", child.Name);
                                //sbEx.Append(Environment.NewLine);
                                //sbEx.Append("return;");
                                //sbEx.Append(Environment.NewLine);
                                //sbEx.Append("}");
                                //sbEx.Append(Environment.NewLine);

                                sbEx.AppendFormat("if (ex is {0}) {{ {1}", child.PropertyType.FullName, System.Environment.NewLine);
                                sbEx.AppendFormat("callback(ThriftMarshaller.Serialize(new TMessage(\"{0}\", TMessageType.Reply, seqID),{1}", childOp.MethodName, System.Environment.NewLine);
                                sbEx.AppendFormat("new {0}{1}_result{{ {4} {2} = ex as {3} }}));{4}return;}} {4}", string.IsNullOrEmpty(childService.DeclaringClassName) ? "" : childService.DeclaringClassName + ".", childOp.MethodName, child.Name, child.PropertyType.FullName, System.Environment.NewLine);
                            }
                            ex = sbEx.ToString();
                        }
                    }

                    string strParams = string.Empty;
                    foreach (var childP in childOp.ListParam)
                    {
                        var childPArr = childP.Split(' ');
                        var aaa = childPArr[childPArr.Length - 1];
                        strParams += "args." + aaa[0].ToString().ToUpper() + aaa.Substring(1) + ",";
                    }
                    if (!string.IsNullOrEmpty(strParams))
                        strParams = strParams.TrimEnd(',') + ", ";

                    if (childOp.ReturnType.ToLower() == "system.void")
                    {
                        strProcessorProcess += this.TlProcessorProcessVoid.Replace("<%=methodName%>", childOp.MethodName)
                            .Replace("<%=strParams%>", strParams)
                            .Replace("<%=setSendArgs%>", GetSetArgsString(childOp.ListParam))
                            .Replace("<%=returnType%>", childOp.ReturnType)
                            .Replace("<%=ex%>", ex)
                            .Replace("<%=declaringClassName%>", string.IsNullOrEmpty(childService.DeclaringClassName) ? "" : childService.DeclaringClassName + ".");
                    }
                    else
                    {
                        strProcessorProcess += this.TlProcessorProcess.Replace("<%=methodName%>", childOp.MethodName)
                            .Replace("<%=strParams%>", strParams)
                            .Replace("<%=setSendArgs%>", GetSetArgsString(childOp.ListParam))
                            .Replace("<%=returnType%>", childOp.ReturnType)
                            .Replace("<%=ex%>", ex)
                            .Replace("<%=declaringClassName%>", string.IsNullOrEmpty(childService.DeclaringClassName) ? "" : childService.DeclaringClassName + ".");
                    }
                }

                var sbProcessMap = new StringBuilder();
                foreach (var childOp in childService.Operations)
                {
                    sbProcessMap.AppendFormat("processMap_[\"{0}\"]={0}_Process;", childOp.MethodName);
                    sbProcessMap.Append(Environment.NewLine);
                }

                sb.Append(this.TlProcessor.Replace("<%=ProcessMethods%>", strProcessorProcess).Replace("<%=ProcessMap%>", sbProcessMap.ToString()));
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                #endregion

                //string fullCode = TlAsyncService.Replace("<%=namespace%>", childService.NameSpace + (string.IsNullOrEmpty(childService.DeclaringClassName) ? "" : "." + childService.DeclaringClassName) + "Proxy")
                string fullCode = TlAsyncService.Replace("<%=namespace%>", childService.NameSpace).Replace("<%=serviceName%>", childService.DeclaringClassName)
                    .Replace("<%=bodyCode%>", sb.ToString());
                list.Add(new CodeFullResult(childService.NameSpace, childService.DeclaringClassName, childService.FullName, fullCode));

                ////写文件
                //var codeFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gen_code");
                //if (!System.IO.Directory.Exists(codeFolder))
                //    System.IO.Directory.CreateDirectory(codeFolder);

                //var codeFilePath = System.IO.Path.Combine(codeFolder, "Async" + (string.IsNullOrEmpty(childService.DeclaringClassName) ? childService.ClassName : childService.DeclaringClassName) + ".cs");
                //if (System.IO.File.Exists(codeFilePath))
                //    System.IO.File.Delete(codeFilePath);

                //System.IO.File.AppendAllText(codeFilePath, fullCode);
            }
            return list;
        }
        #endregion

        static public string GetSetArgsString(List<string> listParam)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < listParam.Count; i++)
            {
                var childParam = listParam[i].Trim().Substring(listParam[i].Trim().LastIndexOf(" ")).Trim();
                sb.AppendFormat("{2}{0} = {1}", (childParam[0]).ToString().ToUpper() + childParam.Remove(0, 1), childParam, i > 0 ? ", " : string.Empty);
            }
            return sb.ToString();
        }
    }
}