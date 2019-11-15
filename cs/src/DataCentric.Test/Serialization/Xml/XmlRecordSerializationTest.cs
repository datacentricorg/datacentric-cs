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
using DataCentric;
using Xunit;

namespace DataCentric.Test
{
    /// <summary>Unit test for XML serialization roundtrip of a record.</summary>
    public class XmlRecordSerializationTest
    {
        /// <summary>Test for serialization of a basic data structure.</summary>
        [Fact]
        public void Basic()
        {
            using (var context = new UnitTestContext(this))
            {
                var obj = new BaseTypeSample();
                obj.SampleName = "ABC";
                obj.DoubleElement = 1.0;

                string xmlString = obj.ToXml();
                string className = obj.GetType().Name;
                context.Log.Verify("Original", xmlString);

                var deserialized = new BaseTypeSample();
                deserialized.ParseXml(xmlString);
                string deserializedString = deserialized.ToXml();
                context.Log.Verify("Copy", deserializedString);
                context.Log.Assert(xmlString == deserializedString, "Serialization roundtrip assert.");
            }
        }

        /// <summary>Test for serialization of a complete data structure.</summary>
        [Fact]
        public void Complete()
        {
            using (var context = new UnitTestContext(this))
            {
                var obj = new DerivedTypeSample();
                obj.SampleName = "AAA";
                obj.DoubleElement = 1.0;
                obj.IntElement = 1;
                obj.KeyElement = new BaseTypeSampleKey();
                obj.KeyElement.SampleName = "BBB";

                obj.NonNullableIntList = new List<int>();
                obj.NonNullableIntList.Add(100);
                obj.NonNullableIntList.Add(200);

                obj.NullableIntList = new List<int?>();
                obj.NullableIntList.Add(100);
                obj.NullableIntList.Add(null);
                obj.NullableIntList.Add(300);

                obj.StringList = new List<string>();
                obj.StringList.Add("AAAA");
                obj.StringList.Add("BBBB");

                obj.KeyList = new List<BaseTypeSampleKey>();
                var keyListElement1 = new BaseTypeSampleKey();
                obj.KeyList.Add(keyListElement1);
                keyListElement1.SampleName = "BBB";
                var keyListElement2 = new BaseTypeSampleKey();
                keyListElement2.SampleName = "BBB";
                obj.KeyList.Add(keyListElement2);

                obj.DataElement = new ElementTypeSample();
                obj.DataElement.SampleName = "CCC";
                obj.DataElement.DoubleElement = 2.0;

                obj.DataList = new List<ElementTypeSample>();
                var dataListItem1 = new ElementTypeSample();
                dataListItem1.SampleName = "DDD";
                dataListItem1.DoubleElement = 3.0;
                obj.DataList.Add(dataListItem1);
                var dataListItem2 = new ElementTypeSample();
                dataListItem2.SampleName = "DDD";
                dataListItem2.DoubleElement = 4.0;
                obj.DataList.Add(dataListItem2);

                string xmlString = obj.ToXml();
                string className = obj.GetType().Name;
                context.Log.Verify("Original", xmlString);

                var deserialized = new DerivedTypeSample();
                deserialized.ParseXml(xmlString);
                string deserializedString = deserialized.ToXml();
                context.Log.Verify("Copy", deserializedString);
                context.Log.Assert(xmlString == deserializedString, "Serialization roundtrip assert.");
            }
        }
    }
}
