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
using System.Runtime.CompilerServices;

namespace DataCentric
{
    /// <summary>
    /// Context for use in test fixtures that require a data source.
    ///
    /// It extends UnitTestContext by creating an empty test
    /// database specific to the test method, and deleting
    /// it after the test exits. The context creates Common
    /// dataset in the database and assigns its TemporalId to
    /// the DataSet property of the context.
    ///
    /// If the test sets KeepTestData = true, the data is retained
    /// after the text exits. This data will be cleared on the
    /// next launch of the test.
    ///
    /// For tests that do not require a data source, use UnitTestContext.
    /// </summary>
    public class MongoTestContext<TDataSource> : UnitTestContext
        where TDataSource : MongoDataSource, IDataSource, new()
    {
        private bool keepTestData_;

        //--- PROPERTIES

        /// <summary>
        /// Name of the unit test method obtained using [CallerMemberName]
        /// attribute of the unit test method signature.
        /// </summary>
        public string CallerMemberName { get; }

        /// <summary>
        /// Path to the unit test source code file obtained using [CallerFilePath]
        /// attribute of the unit test method signature.
        /// </summary>
        public string CallerFilePath { get; }

        //--- CONSTRUCTORS

        /// <summary>
        /// Unit test context for the specified object for the Mongo
        /// server running on the default port of localhost.
        ///
        /// The last two arguments are provided by the compiler unless
        /// specified explicitly by the caller.
        /// </summary>
        public MongoTestContext(
            object obj,
            [CallerMemberName] string methodName = null,
            [CallerFilePath] string sourceFilePath = null)
            :
            this(obj, MongoServerKey.Default, methodName, sourceFilePath)
        {
            // Will use Mongo server running on the default port of localhost
        }

        /// <summary>
        /// Unit test context for the specified object and Mongo server URI.
        ///
        /// The last two arguments are provided by the compiler unless
        /// specified explicitly by the caller.
        /// </summary>
        public MongoTestContext(
            object obj,
            MongoServerKey mongoServerKey,
            [CallerMemberName] string methodName = null,
            [CallerFilePath] string sourceFilePath = null)
            :
            base(obj, methodName, sourceFilePath)
        {
            // Initialize properties of the method context
            // that the unit test method may use.
            CallerMemberName = methodName;
            CallerFilePath = sourceFilePath;

            // Create and initialize data source with TEST instance type.
            //
            // This does not create the database until the data source
            // is actually used to access data.
            string className = obj.GetType().Name;

            // Create data source specified as generic argument
            DataSource = new TDataSource()
            {
                DbName = new DbNameKey()
                {
                    InstanceType = InstanceType.TEST,
                    InstanceName = className,
                    EnvName = methodName
                },
                MongoServer = mongoServerKey
            };

            // Delete (drop) the database to clear the existing data
            DataSource.DeleteDb();

            // Create common dataset and assign it to DataSet property of this context
            DataSet = DataSource.CreateCommon();
        }

        //--- METHODS

        /// <summary>
        /// Releases resources and calls base.Dispose().
        ///
        /// This method will not be called by the garbage collector.
        /// It will only be executed if:
        ///
        /// * This class implements IDisposable; and
        /// * The class instance is created through the using clause
        ///
        /// IMPORTANT - Every override of this method must call base.Dispose()
        /// after executing its own code.
        /// </summary>
        public override void Dispose()
        {
            if (!keepTestData_)
            {
                // Permanently delete the unit test database
                // unless KeepTestData is true
                DataSource.DeleteDb();
            }

            // Dispose base
            base.Dispose();
        }

        /// <summary>
        /// Invoke this method to keep test data after the
        /// test method exits.
        ///
        /// When running under xUnit, the data in test database is not
        /// erased on test method exit if KeepTestData() was invoked.
        ///
        /// When running under DataCentric, the test dataset will not
        /// be deleted on test method exit if KeepTestData() was invoked.
        ///
        /// Note that test data is always erased when test method enters,
        /// irrespective of any KeepTestData() calls and irrespective of
        /// whether or not KeepTestData() has been called.
        /// </summary>
        public override void KeepTestData()
        {
            keepTestData_ = true;
        }
    }
}
