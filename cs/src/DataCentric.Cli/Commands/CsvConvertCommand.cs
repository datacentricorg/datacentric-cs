/*
Copyright (C) 2013-present The DataCentric Authors.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.IO;
using System.Reflection;
using CommandLine;

namespace DataCentric.Cli
{
    [Verb("csv2mongo", HelpText = "Test runner.")]
    public class CsvConvertCommand
    {
        [Option('p', "path", HelpText = "Pattern to filter tests.", Required = true)]
        public string CsvPath { get; set; }

        /// <summary>
        /// Convert records stored in csv format to mongo storage.
        /// </summary>
        public void Execute()
        {
            DbNameKey dbName = new DbNameKey
            {
                EnvType = EnvType.USER, EnvGroup = "TEMP", EnvName = "Default" // TODO - use GUID based DB name
            };

            var dataSource = new TemporalMongoDataSource
            {
                DbName = dbName,
                MongoServer = new MongoServerKey { MongoServerUri = "mongodb://localhost:27017"} // TODO - specify server URI
            };

            Context context = new Context();
            context.DataSource = dataSource;
            context.DataSet = dataSource.CreateCommon();

            // Process all directories inside given folder
            foreach (var dir in Directory.GetDirectories(CsvPath))
            {
                ProcessDirectory(context, dir, context.GetCommon());
            }
        }

        private static void ProcessDirectory(Context context, string path, TemporalId parentDataset)
        {
            var dirName = Path.GetFileName(path);

            // Do not create dataset for Common
            var currentDataset = dirName != "Common"
                                     ? context.CreateDataSet(dirName, context.DataSet)
                                     : parentDataset;

            foreach (var csvFile in Directory.GetFiles(path, "*.csv"))
            {
                var type = Path.GetFileNameWithoutExtension(csvFile);
                Type recordType = ActivatorUtil.ResolveType(type, ActivatorSettings.Assemblies)
                                  ?? throw new ArgumentException($"Type '{type}' not found");

                MethodInfo convertToMongo = typeof(CsvConvertCommand)
                                           .GetMethod(nameof(ConvertCsvToMongo), BindingFlags.Static | BindingFlags.NonPublic)
                                          ?.MakeGenericMethod(recordType);

                convertToMongo?.Invoke(null, new object[] { context, currentDataset, csvFile});
            }

            var directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                ProcessDirectory(context, directory, currentDataset);
            }
        }

        private static void ConvertCsvToMongo<T>(Context context, TemporalId dataset, string csvFile) where T : Record
        {
            string fileContent = File.ReadAllText(csvFile);
            var records = CsvRecordsSerializer<T>.Deserialize(fileContent);

            context.SaveMany(records, dataset);
        }
    }
}