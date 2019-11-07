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
    /// <summary>Unit tests for LocalMinute.</summary>
    public class LocalMinuteTest
    {
        /// <summary>Test roundtrip serialization.</summary>
        [Fact]
        public void Roundtrip()
        {
            using (var context = new UnitTestContext(this))
            {
                VerifyRoundtrip(context, new LocalMinute(0, 0));
                VerifyRoundtrip(context, new LocalMinute(10, 15));
            }
        }

        /// <summary>
        /// Verify that the result of serializing and then deserializing
        /// an object is the same as the original.
        /// </summary>
        private void VerifyRoundtrip(IContext context, LocalMinute value)
        {
            // To be used in assert message
            string nameAsString = value.AsString();

            // Verify string serialization roundtrip
            string stringValue = value.AsString();
            LocalMinute parsedStringValue = LocalMinuteUtil.Parse(stringValue);
            context.Log.Assert(value == parsedStringValue, $"String roundtrip for {nameAsString} assert.");

            // Verify int serialization roundtrip
            int intValue = value.ToIsoInt();
            LocalMinute parsedIntValue = LocalMinuteUtil.FromIsoInt(intValue);
            context.Log.Assert(value == parsedIntValue, $"Int roundtrip for {nameAsString} assert.");
        }
    }
}
