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
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

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
    [BsonSerializer(typeof(BsonKeySerializer<UnitTestKey>))]
    public sealed class UnitTestKey : TypedKey<UnitTestKey, UnitTest>
    {
        /// <summary>
        /// Unique test name.
        ///
        /// The name is set to the fully qualified test class name
        /// in the constructor of this class.
        /// </summary>
        public string TestName { get; set; }
    }
}
