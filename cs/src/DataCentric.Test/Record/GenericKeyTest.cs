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
    /// <summary>Unit tests for GenericKey.</summary>
    public class GenericKeyTest
    {
        /// <summary>Serialization to string followed by deserialization.</summary>
        [Fact]
        public void SerializationRoundtrip()
        {
            using (var context = new UnitTestContext(this))
            {
                var key = new GenericKey {Table = "MyTable", Key = "Token1;Token2"};
                var stringRepresentation = key.ToString();
                var deserializedKey = new GenericKey(stringRepresentation);

                context.Log.Verify($"String Representation={stringRepresentation}");
                context.Log.Verify($"After deserialization={deserializedKey}");
            }
        }
    }
}
