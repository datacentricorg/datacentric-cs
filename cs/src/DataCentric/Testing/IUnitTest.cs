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
using System.Diagnostics;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Makes possible for tests to be executed by:
    ///
    /// * Standard xUnit test runner; or
    /// * Via a DataCentric handler via CLI or in the front end
    ///
    /// This makes it possible to test not only inside the development
    /// environment but also on a deployed version of the application where
    /// access to the xUnit test runner is not available.
    /// </summary>
    public interface IUnitTest
    {
        /// <summary>
        /// Execution context provides access to key resources including:
        ///
        /// * Logging and error reporting
        /// * Cloud calculation service
        /// * Data sources
        /// * Filesystem
        /// * Progress reporting
        /// </summary>
        IContext Context { get; }

        /// <summary>
        /// Unique test name.
        ///
        /// The name is set to the fully qualified test class name
        /// in the constructor of this class.
        /// </summary>
        string TestName { get; }

        /// <summary>
        /// Test complexity level.
        ///
        /// Higher complexity results in more comprehensive testing at
        /// the expect of longer test running times.
        /// </summary>
        TestComplexity? Complexity { get; }

        //--- METHODS

        /// <summary>
        /// Run all methods in this class that have [Fact] or [Theory] attribute.
        ///
        /// This method will run each of the test methods using its own instance
        /// of the test class in parallel.
        /// </summary>
        void RunAll();
    }
}
