namespace VS.Menu.Model
{
    public class DependenceModel
    {
        /// <summary>
        /// 依赖的包
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// 依赖的版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 主命名空间
        /// </summary>
        public string Namespace { get; set; }
    }
}
