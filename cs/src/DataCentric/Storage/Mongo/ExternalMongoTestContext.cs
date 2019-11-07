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

using System.Runtime.CompilerServices;

namespace DataCentric
{
    /// <summary>
    /// ExternalMongoTestContext is the context for use in test
    /// fixtures that will interact with external mongo database.
    ///
    /// It extends UnitTestContext by connecting to the specified
    /// mongo server and picking specified database.
    ///
    /// The context loads an existing Common dataset from data source
    /// and assigns its TemporalId to the DataSet property of the context.
    /// </summary>
    public class ExternalMongoTestContext : UnitTestContext
    {
        /// Unit test context for the specified object and database name.
        ///
        /// The last two arguments are provided by the compiler unless
        /// specified explicitly by the caller.
        public ExternalMongoTestContext(object classInstance, string dbNameString,
                                        [CallerMemberName] string methodName = null,
                                        [CallerFilePath] string sourceFilePath = null) :
            this(classInstance, dbNameString, MongoServerKey.Default, methodName, sourceFilePath)
        {
        }

        /// <summary>
        /// Unit test context for the specified object, database name and Mongo server URI.
        ///
        /// The last two arguments are provided by the compiler unless
        /// specified explicitly by the caller.
        /// </summary>
        public ExternalMongoTestContext(object classInstance, string dbNameString, MongoServerKey serverKey,
                                        [CallerMemberName] string methodName = null,
                                        [CallerFilePath] string sourceFilePath = null) :
            base(classInstance, methodName, sourceFilePath)
        {
            DbNameKey dbName = new DbNameKey();
            dbName.PopulateFrom(dbNameString);

            var dataSource = new TemporalMongoDataSource
            {
                DbName = dbName,
                MongoServer = serverKey
            };

            dataSource.Init(this);

            DataSource = dataSource;

            // Common dataset should already exist in data source, if not then create.
            DataSet = dataSource.GetDataSetOrNull("Common", TemporalId.Empty) ?? dataSource.CreateCommon();
        }
    }
}