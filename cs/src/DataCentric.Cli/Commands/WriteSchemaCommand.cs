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
using System.Collections.Generic;
using CommandLine;

namespace DataCentric.Cli
{
    [Verb("writeSchema")]
    public class WriteSchemaCommand
    {
        [Option('a', "assembly", HelpText = "Paths to assemblies to extract types.")]
        public IEnumerable<string> Assemblies { get; set; }

        [Option('h', "host", Required = true, HelpText = "Db Host")]
        public string Host { get; set; }

        [Option('e', "env", Required = true, HelpText = "Environment type.")]
        public string EnvType { get; set; }

        [Option('g', "group", Required = true, HelpText = "Environment group.")]
        public string EnvGroup { get; set; }

        [Option('n', "name", Required = true, HelpText = "Environment name.")]
        public string EnvName { get; set; }

        public void Execute()
        {
            var schema = new Schema(Assemblies);

            using (var context = GetTargetContext())
            {
                schema.Generate(context);
            }
        }

        private Context GetTargetContext()
        {
            var dataSource = new TemporalMongoDataSource
            {
                EnvType = Enum.Parse<EnvType>(EnvType, true),
                EnvGroup = EnvGroup,
                EnvName = EnvName,
                MongoServer = new MongoServerKey { MongoServerUri = $"mongodb://{Host}" },
            };

            var context = new Context
            {
                DataSource = dataSource, 
                DataSet = TemporalId.Empty
            };

            return context;
        }
    }
}