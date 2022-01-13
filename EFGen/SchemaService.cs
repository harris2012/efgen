using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using MySql.Data.MySqlClient;
using Panosen.DBSchema.Mysql;
using Panosen.DBSchema.Mysql.InformationSchema;

namespace EFGen.Service
{
    public class SchemaService
    {
        public Schema ReadSchema(string connectionString, string dbName)
        {
            Schema schema = new Schema();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var schemaRepository = new SchemaRepository(connection);

                schema.TableList = schemaRepository.GetTables(dbName);
                schema.FieldList = schemaRepository.GetColumns(dbName);
                schema.StatisticsList = schemaRepository.GetStatistics(dbName);
                schema.KeyColumnUsageList = schemaRepository.GetKeyColumnUsages(dbName);
            }

            return schema;
        }
    }

    public class Schema
    {

        /// <summary>
        /// 表
        /// </summary>
        public List<Table> TableList { get; set; }

        /// <summary>
        /// 字段
        /// </summary>
        public List<Column> FieldList { get; set; }

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
