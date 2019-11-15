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
using NodaTime;
using DataCentric;

namespace DataCentric.Test
{
    /// <summary>Unit tests for NodaTime.IsoDayOfWeek extensions.</summary>
    public class IsoDayOfWeekTest
    {
        /// <summary>Test that empty value is recognized by IsEmpty() method.</summary>
        [Fact]
        public void EmptyValue()
        {
            using (var context = new UnitTestContext(this))
            {
                context.Log.Assert(IsoDayOfWeek.None.IsEmpty() == true, "None.IsEmpty() == true");
                context.Log.Assert(IsoDayOfWeek.Monday.IsEmpty() == false, "Monday.IsEmpty() == false");
            }
        }
    }
}
