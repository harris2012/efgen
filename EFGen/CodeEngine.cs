using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using Panosen.Generation;
using Panosen.Resource.CSharp;
using Panosen.CodeDom.CSharpProject.Engine;
using Panosen.CodeDom.CSharpProject;
using Panosen.CodeDom.EFCore.Engine;
using Panosen.CodeDom.EFCore;

namespace EFGen.Generation
{
    public class CodeEngine
    {
        public Package Generate(Context context, CSharpResource cSharpResource)
        {
            var param = context.Param;

            var package = new Package();
            var tableMap = context.TableMap;

            if (!string.IsNullOrEmpty(param.SolutionName))
            {
                package.Add(CSharpResourcePaths._gitattributes, cSharpResource.GetResource(CSharpResourceKeys.__gitattributes));
                package.Add(CSharpResourcePaths._gitignore, cSharpResource.GetResource(CSharpResourceKeys.__gitignore));
            }

            GenerateSolution(param, package);

            GenerateProject(param, package);

            GenerateDBContext(param, package, tableMap);

            GenerateDBContextBuildTable(param, package, tableMap);

            GenerateTableEntity(param, package, tableMap);

            GenerateScript(package, tableMap);

            return package;
        }

        private void GenerateScript(Package package, Dictionary<string, Table> tableMap)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("# Schema").AppendLine();

            foreach (var table in tableMap.Values)
            {
                builder.AppendLine($"## {table.RealTableName}").AppendLine();

                builder.AppendLine("| column | type | comment |");
                builder.AppendLine("| --- | --- | --- |");
                foreach (var column in table.ColumnMap.Values)
                {
                    builder.AppendLine($"| {column.RealColumnName} | {column.ColumnType} | {column.Comment.Replace("\r", "").Replace("\n", ";")} |");
                }

                builder.AppendLine();
            }

            package.Add("efgen.md", builder.ToString());
        }

        private static void GenerateSolution(Param param, Package package)
        {
            if (string.IsNullOrEmpty(param.SolutionName) || string.IsNullOrEmpty(param.ProjectGuid) || string.IsNullOrEmpty(param.SolutionGuid))
            {
                return;
            }
            Solution solution = new Solution();
            solution.SolutionGuid = param.SolutionGuid;
            solution.AddProject(param.ProjectName, param.ProjectGuid, $"{param.ProjectName}\\{param.ProjectName}.csproj", ProjectTypeGuids.CSharpLibrarySDK);

            var builder = new StringBuilder();
            new SolutionEngine().Generate(solution, new StringWriter(builder));
            package.Add($"{param.SolutionName}.sln", builder.ToString());
        }

        private static void GenerateProject(Param param, Package package)
        {
            var project = new Project();
            project.ProjectName = param.ProjectName;
            project.Sdk = "Microsoft.NET.Sdk";
            project.RootNamespace = param.CSharpRootNamespace;
            project.AssemblyName = param.CSharpRootNamespace;
            project.Version = param.Version;
            project.Authors = "harriszhang@live.cn";
            project.GeneratePackageOnBuild = true;
            project.WithDocumentationFile = true;
            project.AddTargetFramework("netstandard2.1");
            project.WithDocumentationFile = true;
            project.AddPackageReference("MySql.EntityFrameworkCore", "5.0.8");

            var builder = new StringBuilder();
            new ProjectEngine().Generate(project, new StringWriter(builder));
            package.Add(Path.Combine(param.ProjectName, $"{param.ProjectName}.csproj"), builder.ToString());
        }

        private static void GenerateDBContext(Param param, Package package, Dictionary<string, Table> tableMap)
        {
            var dbContextFile = new DBContextFile();
            dbContextFile.CSharpRootNamespace = param.CSharpRootNamespace;
            dbContextFile.ContextName = param.ContextName;
            dbContextFile.DBName = param.DBName.ToUpperCamelCase();
            dbContextFile.TableMap = tableMap;

            var builder = new StringBuilder();
            new DBContextEngine().Generate(dbContextFile, new StringWriter(builder));
            package.Add(Path.Combine(param.ProjectName, $"{param.ContextName}.cs"), builder.ToString());
        }

        private static void GenerateDBContextBuildTable(Param param, Package package, Dictionary<string, Table> tableMap)
        {
            foreach (var table in tableMap.Values)
            {
                var dbContextBuildTableFile = new DBContextBuildTableFile();
                dbContextBuildTableFile.CSharpRootNamespace = param.CSharpRootNamespace;
                dbContextBuildTableFile.ContextName = param.ContextName;
                dbContextBuildTableFile.Table = table;
                dbContextBuildTableFile.IgnoreForeignKey = param.IgnoreForeignKey;

                var builder = new StringBuilder();
                new DBContextBuildTableEngine().Generate(dbContextBuildTableFile, new StringWriter(builder));
                package.Add(Path.Combine(param.ProjectName, $"{param.ContextName}_{table.TableName}.cs"), builder.ToString());
            }
        }

        private static void GenerateTableEntity(Param param, Package package, Dictionary<string, Table> tableMap)
        {
            foreach (var table in tableMap.Values)
            {
                var tableEntityFile = new TableEntityFile();
                tableEntityFile.CSharpRootNamespace = param.CSharpRootNamespace;
                tableEntityFile.Table = table;
                tableEntityFile.TableMap = tableMap;
                tableEntityFile.IgnoreForeignKey = param.IgnoreForeignKey;

                var builder = new StringBuilder();
                new TableEntityEngine().Generate(tableEntityFile, new StringWriter(builder));
                package.Add(Path.Combine(param.ProjectName, "Entity", $"{table.TableEntity()}.cs"), builder.ToString());
            }
        }
    }
}
