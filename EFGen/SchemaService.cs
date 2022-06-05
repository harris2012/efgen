using System;
using System.IO;
using System.Text;
using System.Linq;

using MySql.Data.MySqlClient;
using Panosen.DBSchema.Mysql;

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
                schema.ColumnList = schemaRepository.GetColumns(dbName);
                schema.StatisticsList = schemaRepository.GetStatistics(dbName);
                schema.KeyColumnUsageList = schemaRepository.GetKeyColumnUsages(dbName);
            }

            return schema;
        }
    }
}
