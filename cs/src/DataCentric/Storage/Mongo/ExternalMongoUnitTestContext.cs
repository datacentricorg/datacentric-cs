﻿/*
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
    /// ExternalMongoUnitTestContext is the context for use in test
    /// fixtures that will interact with external mongo database.
    ///
    /// It extends UnitTestContext by connecting to the specified
    /// mongo server and picking specified database.
    ///
    /// The context loads an existing Common dataset from data source
    /// and assigns its TemporalId to the DataSet property of the context.
    /// </summary>
    public class ExternalMongoUnitTestContext : UnitTestContext
    {
        /// Unit test context for the specified object and database name.
        ///
        /// The last two arguments are provided by the compiler unless
        /// specified explicitly by the caller.
        public ExternalMongoUnitTestContext(object testObj, string dbNameString,
                                        [CallerMemberName] string methodName = null,
                                        [CallerFilePath] string sourceFilePath = null) :
            this(testObj, dbNameString, MongoServerKey.Default, methodName, sourceFilePath)
        {
        }

        /// <summary>
        /// Unit test context for the specified object, database name and Mongo server URI.
        ///
        /// The last two arguments are provided by the compiler unless
        /// specified explicitly by the caller.
        /// </summary>
        public ExternalMongoUnitTestContext(object testObj, string dbNameString, MongoServerKey serverKey,
                                        [CallerMemberName] string testMethodName = null,
                                        [CallerFilePath] string testSourceFilePath = null) :
            base(testObj, testMethodName, testSourceFilePath)
        {
            var dataSource = new TemporalMongoDataSource
            {
                EnvType = EnvType.Test, // TODO - change to custom
                EnvGroup = null,
                EnvName = dbNameString,
                MongoServer = serverKey
            };

            dataSource.Init(this);

            DataSource = dataSource;

            // Select root dataset
            DataSet = TemporalId.Empty;
        }
    }
}
