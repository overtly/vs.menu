using System.Collections.Generic;

namespace VS.Menu.ThriftGenCore.AsyncGen
{
    /// <summary>
    /// code generator.
    /// </summary>
    public interface ICodeGenerator
    {
        /// <summary>
        /// generate.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        IList<CodeFullResult> Generate(ThriftTemplate template);
    }

    public class CodeFullResult
    {
        public CodeFullResult(string nameSpace, string declaringClassName, string fullName, string code)
        {
            this.NameSpace = nameSpace;
            this.FullName = fullName;
            this.Code = code;
            this.DeclaringClassName = declaringClassName;
        }

        public string NameSpace
        {
            get;
            private set;
        }
        public string DeclaringClassName
        {
            get;
            private set;
        }
        /// <summary>
        /// service name.
        /// </summary>
        public string FullName
        {
            get;
            private set;
        }
        /// <summary>
        /// code
        /// </summary>
        public string Code
        {
            get;
            private set;
        }
    }
}