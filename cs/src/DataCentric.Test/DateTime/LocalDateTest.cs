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
    /// <summary>Unit tests for LocalDate.</summary>
    public class LocalDateTest
    {
        /// <summary>Test properties of the empty (default constructed) value.</summary>
        [Fact]
        public void Empty()
        {
            using (var context = new UnitTestContext(this))
            {
                var empty = new LocalDate();
                context.Log.Assert(empty == LocalDateUtil.Empty, "empty == LocalDateUtil.Empty");
                context.Log.Assert(empty.HasValue() == false, "empty.HasValue() == false");
                context.Log.Assert(empty.ToIsoString() == String.Empty, "empty.ToIsoString() == String.Empty");
                context.Log.Assert(empty.AsString() == String.Empty, "empty.AsString() == String.Empty");
            }
        }

        /// <summary>Test roundtrip serialization.</summary>
        [Fact]
        public void Roundtrip()
        {
            using (var context = new UnitTestContext(this))
            {
                VerifyRoundtrip(context, new LocalDate(2003, 5, 1));
            }
        }

        /// <summary>
        /// Verify that the result of serializing and then deserializing
        /// an object is the same as the original.
        /// </summary>
        private void VerifyRoundtrip(IContext context, LocalDate value)
        {
            // Verify string serialization roundtrip
            string stringValue = value.AsString();
            LocalDate parsedStringValue = LocalDateUtil.Parse(stringValue);
            Assert.Equal(value, parsedStringValue);
            context.Log.Verify($"ISO 8601 format: {stringValue}");

            // Verify int serialization roundtrip
            int intValue = value.ToIsoInt();
            LocalDate parsedIntValue = LocalDateUtil.FromIsoInt(intValue);
            Assert.Equal(value, parsedIntValue);
            context.Log.Verify($"Readable int format: {intValue}");
        }
    }
}
