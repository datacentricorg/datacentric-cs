/*
Copyright (C) 2013-present The DataCentric Authors.
Copyright (C) 2010-present MongoDB Inc.

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
using System.Linq;
using DataCentric;
using MongoDB.Bson; // TODO - remove the remaining use of MongoDB so TemporalId is fully portable
using Xunit;

namespace DataCentric.Test
{
    public class TemporalIdTest
    {
        [Fact]
        public void TestParse()
        {
            // Lowercase and uppercase
            var recordId1 = TemporalId.Parse("2003-05-01T10:15:00.000Z0000010002000abc");
            var recordId2 = TemporalId.Parse("2003-05-01T10:15:00.000Z0000010002000ABC");
            Assert.True(recordId1.ToByteArray().SequenceEqual(recordId2.ToByteArray()));

            // ToString returns lower case
            Assert.True(recordId1.ToString() == "2003-05-01T10:15:00.000Z0000010002000abc");
            Assert.True(recordId1.ToString() == recordId2.ToString());
        }

        [Fact]
        public void TestTryParse()
        {
            // Lowercase and uppercase
            TemporalId recordId1, recordId2;
            Assert.True(TemporalId.TryParse("2003-05-01T10:15:00.000Z0000010002000abc", out recordId1));
            Assert.True(TemporalId.TryParse("2003-05-01T10:15:00.000Z0000010002000ABC", out recordId2));
            Assert.True(recordId1.ToByteArray().SequenceEqual(recordId2.ToByteArray()));

            // ToString returns lower case
            Assert.True(recordId1.ToString() == "2003-05-01T10:15:00.000Z0000010002000abc");
            Assert.True(recordId1.ToString() == recordId2.ToString());
        }
    }
}
