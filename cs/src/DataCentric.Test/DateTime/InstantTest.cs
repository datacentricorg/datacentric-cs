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
using NodaTime;

namespace DataCentric.Test
{
    /// <summary>Unit tests for NodaTime.Instant.</summary>
    public class InstantTest
    {
        /// <summary>Test properties of the empty (default constructed) value.</summary>
        [Fact]
        public void Empty()
        {
            using (var context = new UnitTestContext(this))
            {
                var empty = new Instant();
                context.Log.Assert(empty == InstantUtil.Empty, "empty == InstantUtil.Empty");
                context.Log.Assert(empty.HasValue() == false, "empty.HasValue() == false");
                context.Log.Assert(empty.ToIsoString() == String.Empty, "empty.ToIsoString() == String.Empty");
                context.Log.Assert(empty.AsString() == String.Empty, "empty.AsString() == String.Empty");
            }
        }

        /// <summary>Test detecting whole vs. fractional minutes, seconds, or milliseconds.</summary>
        [Fact]
        public void DetectWhole()
        {
            using (var context = new UnitTestContext(this))
            {
                context.Log.Assert(InstantUtil.Utc(2003, 5, 1, 10, 15, 30).IsMillisecond(), "Whole milliseconds");
                context.Log.Assert(!InstantUtil.Utc(2003, 5, 1, 10, 15, 30).PlusNanoseconds(100000).IsMillisecond(), "Fractional milliseconds");
                context.Log.Assert(InstantUtil.Utc(2003, 5, 1, 10, 15, 30, 0).IsSecond(), "Whole seconds");
                context.Log.Assert(!InstantUtil.Utc(2003, 5, 1, 10, 15, 30, 1).IsSecond(), "Fractional seconds");
                context.Log.Assert(InstantUtil.Utc(2003, 5, 1, 10, 15, 0).IsMinute(), "Whole minutes");
                context.Log.Assert(!InstantUtil.Utc(2003, 5, 1, 10, 15, 1, 0).IsMinute(), "Fractional minutes");
            }
        }

        /// <summary>Test roundtrip serialization.</summary>
        [Fact]
        public void Roundtrip()
        {
            using (var context = new UnitTestContext(this))
            {
                VerifyRoundtrip(context, new LocalDateTime(2003, 5, 1, 10, 15, 30).ToInstant(DateTimeZone.Utc));
                VerifyRoundtrip(context, new LocalDateTime(2003, 5, 1, 10, 15, 30, 5).ToInstant(DateTimeZone.Utc));
            }
        }

        /// <summary>
        /// Verify that the result of serializing and then deserializing
        /// an object is the same as the original.
        /// </summary>
        private void VerifyRoundtrip(IContext context, Instant value)
        {
            // To be used in assert message
            string nameAsString = value.AsString();

            // Verify string serialization roundtrip
            string stringValue = value.AsString();
            Instant parsedStringValue = InstantUtil.Parse(stringValue);
            context.Log.Assert(value == parsedStringValue, $"String roundtrip for {nameAsString} assert.");

            // Verify long serialization roundtrip
            long longValue = value.ToIsoLong();
            Instant parsedLongValue = InstantUtil.FromIsoLong(longValue);
            context.Log.Assert(value == parsedLongValue, $"Long roundtrip for {nameAsString} assert.");
        }
    }
}
