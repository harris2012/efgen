using EFGen.Generation;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFGen.Service
{
    public class Profile
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DBName { get; set; }

        /////// <summary>
        /////// 目标解决方案文件夹
        /////// </summary>
        ////public string TargetSolutionFolder { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public Param Param { get; set; }
    }
}
