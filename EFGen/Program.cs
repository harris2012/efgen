using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EFGen.Generation;
using EFGen.Service;
using Newtonsoft.Json;
using EFGen.Helper;
using Panosen.Generation;
using Panosen.Resource.CSharp;

namespace EFGen
{
    class Program
    {

        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("operation is required.");
                Console.WriteLine("1. efgen init 生成efgen.json模版文件");
                Console.WriteLine("2. efgen gen 生成代码");
                return;
            }

            if ("init".Equals(args[0], StringComparison.OrdinalIgnoreCase))
            {
                var profiles = new List<Profile>();
                profiles.Add(new Profile { Param = new Param() });

                File.WriteAllText("efgen.json", JsonConvert.SerializeObject(profiles, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include
                }));

                return;
            }

            if ("gen".Equals(args[0], StringComparison.OrdinalIgnoreCase))
            {
                if (!File.Exists("efgen.json"))
                {
                    Console.WriteLine("efgen.json is required.");
                    return;
                }

                string text = File.ReadAllText("efgen.json");
                if (text.StartsWith("["))
                {
                    var profiles = JsonConvert.DeserializeObject<List<Profile>>(text);
                    foreach (var profile in profiles)
                    {
                        Generate(profile);
                    }
                }
                else
                {
                    var profile = JsonConvert.DeserializeObject<Profile>(text);

                    Generate(profile);
                }
            }

            Console.WriteLine("thank you.");
        }

        private static void Generate(Profile profile)
        {
            var schema = new SchemaService().ReadSchema(profile.ConnectionString, profile.DBName);

            var targetSolutionFolder = Environment.CurrentDirectory;

            var tableMap = new Mapper().BuildTableMap(schema.TableList, schema.ColumnList, schema.StatisticsList, schema.KeyColumnUsageList, profile.Param.TablePrefix);

            var cSharpResource = new CSharpResource();

            var package = new Package();

            new CodeEngine().Generate(package, profile.Param, tableMap, cSharpResource);

            new ScriptEngine().Generate(package, Path.Combine("efgen", $"{profile.Param.DBName}.sql"), schema);

            foreach (var file in package.Files)
            {
                switch (file.ContentType)
                {
                    case ContentType.String:
                        {
                            var bytes = DefaultEncoding.GetBytes((file as PlainFile).Content);
                            WriteToFile(targetSolutionFolder, file.FilePath, bytes);
                        }
                        break;
                    case ContentType.Bytes:
                        {
                            var bytes = (file as BytesFile).Bytes;
                            WriteToFile(targetSolutionFolder, file.FilePath, bytes);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void WriteToFile(string targetFolder, string filePath, byte[] bytes)
        {
            var fileName = Path.Combine(targetFolder, filePath);
            if (File.Exists(fileName))
            {
                if (HashHelper.ComputeHash(File.ReadAllBytes(fileName)) == HashHelper.ComputeHash(bytes))
                {
                    return;
                }
            }

            var fileDirectory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            File.WriteAllBytes(fileName, bytes);
        }
    }
}
