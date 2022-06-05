using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Panosen.CodeDom.Mysql;
using Panosen.CodeDom.Mysql.Engine;
using Panosen.Generation;

using SchemaTable = Panosen.DBSchema.Mysql.InformationSchema.Table;
using SchemaColumn = Panosen.DBSchema.Mysql.InformationSchema.Column;

namespace EFGen
{
    public class ScriptEngine
    {
        public void Generate(Package package, string fileName, Schema schema)
        {
            StringBuilder builder = new StringBuilder();

            GenerateTables(builder, schema.TableList, schema.ColumnList);

            package.Add(fileName, builder.ToString());
        }

        private void GenerateTables(StringBuilder builder, List<SchemaTable> schemaTableList, List<SchemaColumn> schemaColumnList)
        {
            if (schemaTableList == null || schemaTableList.Count == 0)
            {
                return;
            }

            foreach (var schemaTable in schemaTableList)
            {
                var createTable = new CreateTable();
                createTable.Name = schemaTable.TABLE_NAME;
                createTable.Comment = schemaTable.TABLE_COMMENT;

                var schemaColumns = schemaColumnList
                    .Where(v => v.TABLE_NAME == schemaTable.TABLE_NAME)
                    .OrderBy(v => v.ORDINAL_POSITION)
                    .ToList();
                if (schemaColumns.Count > 0)
                {
                    foreach (var schemaColumn in schemaColumns)
                    {
                        var field = createTable.AddField(schemaColumn.COLUMN_NAME);
                        field.ColumnType = schemaColumn.COLUMN_TYPE;
                        field.NotNull = "NO".Equals(schemaColumn.IS_NULLABLE);
                        field.Comment = schemaColumn.COLUMN_COMMENT;
                        if (schemaColumn.COLUMN_DEFAULT != null)
                        {
                            if ("CURRENT_TIMESTAMP".Equals(schemaColumn.COLUMN_DEFAULT))
                            {
                                field.DefaultValue = schemaColumn.COLUMN_DEFAULT;
                            }
                            else
                            {
                                field.DefaultValue = "'" + schemaColumn.COLUMN_DEFAULT + "'";
                            }
                        }
                        field.CharacterSet = schemaColumn.CHARACTER_SET_NAME;
                        field.Collate = schemaColumn.COLLATION_NAME;

                        if (schemaColumn.EXTRA != null)
                        {
                            if (schemaColumn.EXTRA.Contains("auto_increment"))
                            {
                                field.AutoIncrement = true;
                            }
                            if (schemaColumn.EXTRA != null && schemaColumn.EXTRA.Contains("on update CURRENT_TIMESTAMP"))
                            {
                                field.OnUpdate = "CURRENT_TIMESTAMP";
                            }
                        }
                    }

                    var primaryColumn = schemaColumns.FirstOrDefault(v => v.COLUMN_KEY == "PRI");
                    if (primaryColumn != null)
                    {
                        createTable.PrimaryKey = primaryColumn.COLUMN_NAME;
                    }
                }

                builder.AppendLine(createTable.TransformText());
            }
        }
    }
}
