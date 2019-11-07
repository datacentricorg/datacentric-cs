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
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Xunit;
using DataCentric;

namespace DataCentric.Test
{
    /// <summary>Test key class.</summary>
    [BsonSerializer(typeof(BsonKeySerializer<FlagTestKey>))]
    public sealed class FlagTestKey : TypedKey<FlagTestKey, FlagTestRecord>
    {
        /// <summary>Sample field.</summary>
        public string Name { get; set; }
    }

    /// <summary>Test record class</summary>
    public class FlagTestRecord : TypedRecord<FlagTestKey, FlagTestRecord>
    {
        /// <summary>Sample field.</summary>
        [BsonRequired]
        public string Name { get; set; }

        /// <summary>Sample field.</summary>
        public Flag? FlagValue { get; set; }
    }

    /// <summary>Unit test for Query.</summary>
    public class FlagTest : UnitTest
    {
        /// <summary>Test query by flag value.</summary>
        [Fact]
        public void Query()
        {
            using (var context = CreateMethodContext())
            {
                context.KeepTestData();

                var recordOff = new FlagTestRecord() {Name = "Off"};
                var recordOn = new FlagTestRecord() { Name = "On", FlagValue = Flag.True};
                context.SaveMany(new List<FlagTestRecord>{ recordOff, recordOn });

                if (true)
                {
                    // Query for all records without restrictions,
                    // should return 2 records
                    var query = context.DataSource.GetQuery<FlagTestRecord>(context.DataSet);

                    context.Log.Verify("Unconstrained query");
                    foreach (var obj in query.AsEnumerable())
                    {
                        context.Log.Verify($"    Key={obj.Key} Flag={obj.FlagValue}");
                    }
                }

                if (true)
                {
                    // Query for all records without restrictions,
                    // should return 1 out of 8 records

                    var query = context.DataSource.GetQuery<FlagTestRecord>(context.DataSet)
                        .Where(p => p.FlagValue == Flag.True);

                    context.Log.Verify("Constrained query");
                    foreach (var obj in query.AsEnumerable())
                    {
                        context.Log.Verify($"    Key={obj.Key} Flag={obj.FlagValue}");
                    }
                }
            }
        }
    }
}
