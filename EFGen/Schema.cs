using System.Collections.Generic;
using Panosen.DBSchema.Mysql.InformationSchema;

namespace EFGen
{
    public class Schema
    {

        /// <summary>
        /// 表
        /// </summary>
        public List<Table> TableList { get; set; }

        /// <summary>
        /// 字段
        /// </summary>
        public List<Column> ColumnList { get; set; }

        /// <summary>
        /// 外键
        /// </summary>
        public List<KeyColumnUsage> KeyColumnUsageList { get; set; }

        /// <summary>
        /// 索引
        /// </summary>
        public List<Statistics> StatisticsList { get; set; }
    }
}
