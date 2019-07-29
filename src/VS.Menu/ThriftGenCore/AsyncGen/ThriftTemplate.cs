using System.Collections.Generic;

namespace VS.Menu.ThriftGenCore.AsyncGen
{
    /// <summary>
    /// thrift template
    /// </summary>
    public class ThriftTemplate
    {
        private List<Service> _services = new List<Service>();

        /// <summary>
        /// new
        /// </summary>
        /// <param name="namesapce"></param>
        public ThriftTemplate()
        {
        }

        /// <summary>
        /// add service
        /// </summary>
        /// <param name="service"></param>
        public void AddService(Service service)
        {
            this._services.Add(service);
        }

        /// <summary>
        /// get service
        /// </summary>
        public List<Service> Services
        {
            get { return this._services; }
        }

        /// <summary>
        /// thrift service
        /// </summary>
        public class Service
        {
            private string _namespace = null;
            private string _className = null;
            private string _fullName = null;
            private string _declaringClassName = null;
            private List<Operation> _operations = new List<Operation>();
            private System.Reflection.Assembly _assembly = null;

            public Service(string nameSpace, string className, string fullName, string declaringClassName, System.Reflection.Assembly assembly)
            {
                this._namespace = nameSpace;
                this._className = className;
                this._fullName = fullName;
                this._declaringClassName = declaringClassName;
                this._assembly = assembly;
            }
            /// <summary>
            /// add
            /// </summary>
            /// <param name="operation"></param>
            public void AddOperation(Operation operation)
            {
                this._operations.Add(operation);
            }

            public string NameSpace
            {
                get { return this._namespace; }
            }
            /// <summary>
            /// get class name
            /// </summary>
            public string ClassName
            {
                get { return this._className; }
            }
            /// <summary>
            /// fullname
            /// </summary>
            public string FullName
            {
                get { return this._fullName; }
            }
            public string DeclaringClassName
            {
                get { return this._declaringClassName; }
            }
            /// <summary>
            /// Operations
            /// </summary>
            public List<Operation> Operations
            {
                get { return this._operations; }
            }
            /// <summary>
            /// get assembly
            /// </summary>
            public System.Reflection.Assembly Assembly
            {
                get { return this._assembly; }
            }
        }

        /// <summary>
        /// Operations
        /// yaofeng add _summary
        /// </summary>
        public class Operation
        {
            private string _returnType, _methodName, _summary;
            private List<string> _listParam = null;

            /// <summary>
            /// new
            /// </summary>
            /// <param name="returnType"></param>
            /// <param name="methodName"></param>
            /// <param name="listParam"></param>
            public Operation(string returnType, string methodName, List<string> listParam, string summary = "")
            {
                this._returnType = returnType;
                this._methodName = methodName;
                this._listParam = listParam;
                this._summary = summary;
            }

            /// <summary>
            /// get return type.
            /// </summary>
            public string ReturnType
            {
                get { return this._returnType; }
            }
            /// <summary>
            /// get method name
            /// </summary>
            public string MethodName
            {
                get { return this._methodName; }
            }
            /// <summary>
            /// params
            /// </summary>
            public List<string> ListParam
            {
                get { return this._listParam; }
            }

            /// <summary>
            /// summary
            /// </summary>
            public string Summary
            {
                get { return this._summary; }
            }
        }
    }
}