using System;
using System.Collections.Generic;
using System.Text;

namespace EFGen.Generation
{
    public class Param
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 根命名空间
        /// </summary>
        public string CSharpRootNamespace { get; set; }

        /// <summary>
        /// 目标上下文名称
        /// </summary>
        public string ContextName { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// 不使用外键
        /// </summary>
        public bool IgnoreForeignKey { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 需要忽略的表格前缀
        /// </summary>
        public string TablePrefix { get; set; }


        #region 用于生成解决方案文件

        /// <summary>
        /// 解决方案名称
        /// </summary>
        public string SolutionName { get; set; }

        /// <summary>
        /// 解决方案GUID
        /// </summary>
        public string SolutionGuid { get; set; }

        /// <summary>
        /// 项目GUID
        /// </summary>
        public string ProjectGuid { get; set; }

        #endregion
    }
}
