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
using System.Linq;
using System.Reflection;
using CommandLine;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace DataCentric.Cli
{
    [Verb("run", HelpText = "Execute handler.")]
    public class RunCommand
    {
        [Option('s', "source", Required = true, HelpText = "Source environment - folder for file storage and connection string for DB.")]
        public string Source { get; set; }

        [Option('e', "env", Required = true, HelpText = "Environment type.")]
        public string EnvType { get; set; }

        [Option('g', "group", Required = true, HelpText = "Environment group.")]
        public string EnvGroup { get; set; }

        [Option('n', "name", Required = true, HelpText = "Environment name.")]
        public string EnvName { get; set; }

        [Option('d', "dataset", Required = true, HelpText = "Setting specifies data set name.")]
        public string Dataset { get; set; }

        [Option('k', "key", Required = true, HelpText = "Key of entity.")]
        public string Key { get; set; }

        [Option('t', "type", Required = true, HelpText = "Type handler belongs.")]
        public string Type { get; set; }

        [Option('h', "handler", Required = true, HelpText = "Handler name to execute.")]
        public string Handler { get; set; }

        [Option('a', "arguments", HelpText = "Space separated handler arguments in name=value format.")]
        public IEnumerable<string> Arguments { get; set; }

        /// <summary>
        /// Corresponds to CLI "run" keyword. Executes handler specified by run options.
        /// </summary>
        public void Execute()
        {
            Type recordType = ActivatorUtil.ResolveType(Type, ActivatorSettings.Assemblies)
                              ?? throw new ArgumentException($"Type '{Type}' not found");

            // Register type in BsonClassMap if not yet registered.
            // This part is critical for derived types, since collection creation registers only base type.
            if (!BsonClassMap.IsClassMapRegistered(recordType))
            {
                MethodInfo registerMap = typeof(BsonClassMap)
                                        .GetMethod(nameof(BsonClassMap.RegisterClassMap), new Type[] { })
                                        .MakeGenericMethod(recordType);
                registerMap.Invoke(null, new object[] { });
            }

            (Type keyType, Type baseRecordType) = GetRecordTypeArguments(recordType);

            MethodInfo createHandlerMethod = typeof(RunCommand)
                                            .GetMethod(nameof(CreateHandler), BindingFlags.Static | BindingFlags.NonPublic)
                                            .MakeGenericMethod(keyType, baseRecordType, recordType);

            // TODO: changes to db naming should be synchronized with client, afterwards connection should be parsed from source
            string connectionLiteral = "ConnectionString=";

            // Extract ConnectionString from source string
            string[] sourceParams = Source.Split(',');
            string connectionString = sourceParams.First(t => t.StartsWith(connectionLiteral)).Substring(connectionLiteral.Length);

            // Convert connection string to db name and hosts
            MongoUrl url = MongoUrl.Create(connectionString);

            // Use case insensitive parsing for the environment type
            var dataSource = new TemporalMongoDataSource
            {
                EnvType = Enum.Parse<EnvType>(EnvType, true),
                EnvGroup = EnvGroup,
                EnvName = EnvName,
                MongoServer = new MongoServerKey { MongoServerUri = $"mongodb://{url.Server}"}
            };

            var context = new Context();
            context.DataSource = dataSource;
            context.DataSet = TemporalId.Empty;

            object record = createHandlerMethod.Invoke(null, new object[] { context, this });

            MethodInfo handlerMethod = record.GetType().GetMethod(Handler)
                                       ?? throw new ArgumentException($"Method '{Handler}' not found");

            // Check that method has [HandlerMethod] attribute before calling it.
            if (handlerMethod.GetCustomAttribute<HandlerMethodAttribute>() == null)
                throw new Exception($"Cannot run {Handler} method, missing [HandlerMethod] attribute.");

            handlerMethod.Invoke(record, ActivatorUtil.CreateParameterValues(handlerMethod, Arguments));
        }

        /// <summary>
        /// Helper method to create and init instance of handler class.
        /// </summary>
        private static TRecord CreateHandler<TKey, TBaseRecord, TRecord>(Context context, RunCommand command)
            where TKey : TypedKey<TKey, TBaseRecord>, new()
            where TRecord : TBaseRecord
            where TBaseRecord : TypedRecord<TKey, TBaseRecord>
        {
            TKey key = Activator.CreateInstance<TKey>();
            key.PopulateFrom(command.Key);

            TemporalId dataSet = context.GetDataSet(command.Dataset);
            TRecord record = (TRecord) context.LoadOrNull(key, dataSet);

            record.Init(context);

            return record;
        }

        /// <summary>
        /// Search Record in type hierarchy and returns its type arguments.
        /// </summary>
        private static (Type,Type) GetRecordTypeArguments(Type type)
        {
            while (type.BaseType != null)
            {
                type = type.BaseType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(TypedRecord<,>))
                {
                    return (type.GetGenericArgument(0), type.GetGenericArgument(1));
                }
            }
            throw new InvalidOperationException("Base TypedRecord<,> type was not found");
        }
    }
}