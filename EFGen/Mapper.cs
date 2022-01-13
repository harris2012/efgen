using System;
using System.IO;
using System.Text;
using System.Linq;

using Panosen.DBSchema.Mysql.InformationSchema;
using System.Collections.Generic;

using EFCoreTable = Panosen.CodeDom.EFCore.Table;
using EFCoreColumn = Panosen.CodeDom.EFCore.Column;
using EFCoreKeyColumnUsage = Panosen.CodeDom.EFCore.KeyColumnUsage;

using SchemaTable = Panosen.DBSchema.Mysql.InformationSchema.Table;
using SchemaColumn = Panosen.DBSchema.Mysql.InformationSchema.Column;
using SchemaKeyColumnUsage = Panosen.DBSchema.Mysql.InformationSchema.KeyColumnUsage;

using Panosen.Language.Mysql;
using Panosen.Language.CSharp;

namespace EFGen.Service
{
    public class Mapper
    {
        public Dictionary<string, EFCoreTable> BuildTableMap(
            List<SchemaTable> schemaTableList,
            List<SchemaColumn> fieldEntityList,
            List<Statistics> statisticsList,
            List<SchemaKeyColumnUsage> schemaKeyColumnUsageList,
            string tablePrefix)
        {
            Dictionary<string, EFCoreTable> tableMap = new Dictionary<string, EFCoreTable>();

            foreach (var schemaTableEntity in schemaTableList)
            {
                EFCoreTable efCoreTable = new EFCoreTable();

                var tempTableName = schemaTableEntity.TABLE_NAME;
                if (!string.IsNullOrEmpty(tablePrefix) && tempTableName.StartsWith(tablePrefix))
                {
                    tempTableName = tempTableName.Substring(tablePrefix.Length);
                }
                efCoreTable.TableName = tempTableName.ToUpperCamelCase();
                efCoreTable.RealTableName = schemaTableEntity.TABLE_NAME;

                efCoreTable.ColumnMap = fieldEntityList
                    .Where(v => v.TABLE_NAME == schemaTableEntity.TABLE_NAME)
                    .OrderBy(v => v.ORDINAL_POSITION)
                    .Select(v => ToProperty(v))
                    .ToDictionary(v => v.ColumnName, v => v);

                tableMap.Add(efCoreTable.TableName, efCoreTable);
            }

            var keyColumnUsageList = schemaKeyColumnUsageList.OrderBy(v => v.CONSTRAINT_NAME).Select(v => new EFCoreKeyColumnUsage
            {
                ConstraintName = v.CONSTRAINT_NAME,

                TableName = v.TABLE_NAME != null ? v.TABLE_NAME.ToUpperCamelCase() : null,
                RealTableName = v.TABLE_NAME,

                ColumnName = v.COLUMN_NAME != null ? v.COLUMN_NAME.ToUpperCamelCase() : null,
                RealColumnName = v.COLUMN_NAME,

                ReferencedTableName = v.REFERENCED_TABLE_NAME != null ? v.REFERENCED_TABLE_NAME.ToUpperCamelCase() : null,
                RealReferencedTableName = v.REFERENCED_TABLE_NAME,

                ReferencedColumnName = v.REFERENCED_COLUMN_NAME != null ? v.REFERENCED_COLUMN_NAME.ToUpperCamelCase() : null,
                RealReferencedColumnName = v.REFERENCED_COLUMN_NAME

            }).ToList();

            foreach (var table in tableMap.Values)
            {
                //主键
                table.PrimaryKeyColumns = statisticsList
                    .Where(v => v.TABLE_NAME == table.RealTableName && v.INDEX_NAME == "PRIMARY")
                    .OrderBy(v => v.SEQ_IN_INDEX)
                    .Select(v => table.ColumnMap.Values.First(p => p.RealColumnName == v.COLUMN_NAME))
                    .ToList();

                //索引
                table.Indexes = statisticsList
                    .Where(v => v.TABLE_NAME == table.RealTableName && v.INDEX_NAME != "PRIMARY")
                    .GroupBy(v => v.INDEX_NAME)
                    .Select(v => new Panosen.CodeDom.EFCore.Index
                    {
                        Name = v.Key,
                        NONE_UNIQUE = v.First().NON_UNIQUE,
                        Properties = v.Select(x => table.ColumnMap.Values.First(p => p.RealColumnName == x.COLUMN_NAME)).ToList()
                    }).ToList();

                //外键
                table.KeyColumnUsageList = keyColumnUsageList;
            }

            return tableMap;
        }

        /// <summary>
        /// 添加一个属性
        /// </summary>
        private EFCoreColumn ToProperty(SchemaColumn schemaColumn)
        {
            EFCoreColumn efCoreColumn = new EFCoreColumn();
            efCoreColumn.ColumnName = schemaColumn.COLUMN_NAME.ToUpperCamelCase();
            efCoreColumn.RealColumnName = schemaColumn.COLUMN_NAME;
            efCoreColumn.CSharpType = ToCSharpType(schemaColumn);
            efCoreColumn.MaxLength = (uint?)schemaColumn.CHARACTER_MAXIMUM_LENGTH;
            efCoreColumn.NotNullable = schemaColumn.IS_NULLABLE == "NO";
            efCoreColumn.ColumnType = schemaColumn.COLUMN_TYPE;
            efCoreColumn.Comment = schemaColumn.COLUMN_COMMENT;

            return efCoreColumn;
        }

        private static string ToCSharpType(SchemaColumn column)
        {
            switch (column.DATA_TYPE)
            {
                case MysqlDataTypeConstant.INT:
                    return column.IS_NULLABLE == "YES" ? CSharpTypeConstant._INT : CSharpTypeConstant.INT;

                case MysqlDataTypeConstant.BIGINT:
                    return column.IS_NULLABLE == "YES" ? CSharpTypeConstant._LONG : CSharpTypeConstant.LONG;

                case MysqlDataTypeConstant.DOUBLE:
                    return column.IS_NULLABLE == "YES" ? CSharpTypeConstant._DOUBLE : CSharpTypeConstant.DOUBLE;

                case MysqlDataTypeConstant.SMALLINT:
                    return column.IS_NULLABLE == "YES" ? CSharpTypeConstant._SHORT : CSharpTypeConstant.SHORT;

                case MysqlDataTypeConstant.CHAR:
                case MysqlDataTypeConstant.VARCHAR:
                case MysqlDataTypeConstant.TEXT:
                case MysqlDataTypeConstant.TINYTEXT:
                case MysqlDataTypeConstant.MEDIUMTEXT:
                case MysqlDataTypeConstant.LONGTEXT:
                    return CSharpTypeConstant.STRING;

                case MysqlDataTypeConstant.TINYINT:
                    if (column.COLUMN_TYPE == "tinyint(1)")
                    {
                        return column.IS_NULLABLE == "YES" ? CSharpTypeConstant._BOOL : CSharpTypeConstant.BOOL;
                    }
                    else
                    {
                        return column.IS_NULLABLE == "YES" ? CSharpTypeConstant._BYTE : CSharpTypeConstant.BYTE;
                    }

                case MysqlDataTypeConstant.DATETIME:
                    return column.IS_NULLABLE == "YES" ? CSharpTypeConstant._DATETIME : CSharpTypeConstant.DATETIME;

                case MysqlDataTypeConstant.TIMESTAMP:
                    return column.IS_NULLABLE == "YES" ? CSharpTypeConstant._DATETIMEOFFSET : CSharpTypeConstant.DATETIMEOFFSET;

                case MysqlDataTypeConstant.DECIMAL:
                    return column.IS_NULLABLE == "YES" ? CSharpTypeConstant._DECIMAL : CSharpTypeConstant.DECIMAL;

                case MysqlDataTypeConstant.MEDIUMINT:
                case MysqlDataTypeConstant.FLOAT:
                case MysqlDataTypeConstant.DATE:
                case MysqlDataTypeConstant.TIME:
                case MysqlDataTypeConstant.YEAR:
                case MysqlDataTypeConstant.TINYBLOB:
                case MysqlDataTypeConstant.BLOB:
                case MysqlDataTypeConstant.MEDIUMBLOB:
                case MysqlDataTypeConstant.LONGBLOB:
                default:
                    return "_";
            }
        }
    }
}
