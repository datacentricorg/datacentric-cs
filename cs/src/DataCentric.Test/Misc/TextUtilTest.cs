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
using Xunit;
using DataCentric;

namespace DataCentric.Test
{
    /// <summary>Unit tests for TextUtil.</summary>
    public class TextUtilTest
    {
        /// <summary>Test for GenerateRandomStrings method.</summary>
        [Fact]
        public void GenerateRandomStrings()
        {
            using (var context = new UnitTestContext(this))
            {
                // Two short strings, seed 0
                List<string> result1 = TextUtil.GenerateRandomStrings(2, 3, 0);
                context.Log.Verify($"Seed 0: {string.Join(";", result1)}");

                // Confirm that generated values change with seed
                List<string> result2 = TextUtil.GenerateRandomStrings(2, 3, 1);
                context.Log.Verify($"Seed 0: {string.Join(";", result2)}");

                // Confirm that the generator works for string length exceeding alphabet size
                List<string> result3 = TextUtil.GenerateRandomStrings(1, 50, 0);
                context.Log.Verify($"Long string: {string.Join(";",result3)}");
            }
        }
    }
}
