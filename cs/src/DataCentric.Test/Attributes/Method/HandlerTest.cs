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
using Xunit;
using DataCentric;

namespace DataCentric.Test
{
    /// <summary>Test for HandlerMethod attribute.</summary>
    public class HandlerTest
    {
        /// <summary>
        /// In this test, we invoke handlers in process to create baseline output
        /// that will be compared to the output of the CLI call.
        /// </summary>
        [Fact]
        public void InProcess()
        {
            using (var context = new UnitTestContext(this))
            {
                // Create base instance
                var baseSampleData = new BaseSampleData();
                baseSampleData.RecordName = "InProcess";
                baseSampleData.RecordIndex = 1;
                baseSampleData.Init(context);

                // Invoke handlers of the base class
                baseSampleData.NonVirtualBaseHandler();
                baseSampleData.VirtualBaseHandler();

                // Create derived instance
                var derivedSampleData = new DerivedSampleData();
                derivedSampleData.Init(context);

                // Invoke handlers of the derived class
                derivedSampleData.NonVirtualDerivedHandler();
                derivedSampleData.VirtualBaseHandler();
            }
        }
    }
}
