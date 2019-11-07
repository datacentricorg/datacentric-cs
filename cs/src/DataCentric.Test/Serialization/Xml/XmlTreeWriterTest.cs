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
    /// <summary>Unit tests for XmlTreeWriter.</summary>
    public class XmlTreeWriterTest
    {
        /// <summary>Test for writing XML with attributes.</summary>
        [Fact]
        public void Attributes()
        {
            using (var context = new UnitTestContext(this))
            {
                // Create XML writer
                ITreeWriter writer = new XmlTreeWriter();

                // Add root element with two attributes
                writer.WriteStartDocument("rootElement");
                writer.WriteStartDict();
                writer.WriteAttribute("attributeOfRootElement1", "AttributeValue1");
                writer.WriteAttribute("attributeOfRootElement2", "AttributeValue2");

                // Add embedded element
                writer.WriteStartDictElement("secondElement");
                writer.WriteAttribute("attributeOfSecondElement1", "AttributeValue1");
                writer.WriteAttribute("attributeOfSecondElement2", "AttributeValue2");

                // Add value element without attributes
                writer.WriteValueElement("valueElement1", "TestValue1");

                // Add value array without attributes
                writer.WriteValueArray("valueArray1", new string[] {"TestValue2", "TestValue3"});

                // Add value element with two attributes
                writer.WriteStartElement("valueElement2");
                writer.WriteStartValue();
                writer.WriteAttribute("attributeOfValueElement1", "AttributeValue1");
                writer.WriteAttribute("attributeOfValueElement2", "AttributeValue2");
                writer.WriteValue("TestValue2");
                writer.WriteEndValue();
                writer.WriteEndElement("valueElement2");

                // Add value array with two attributes for each element
                writer.WriteStartElement("valueArray2");
                writer.WriteStartArray();

                writer.WriteStartArrayItem();
                writer.WriteStartValue();
                writer.WriteAttribute("attributeOfValueArray1", "AttributeValue1");
                writer.WriteAttribute("attributeOfValueArray2", "AttributeValue2");
                writer.WriteValue("TestValue2");
                writer.WriteEndValue();
                writer.WriteEndArrayItem();

                writer.WriteStartArrayItem();
                writer.WriteStartValue();
                writer.WriteAttribute("attributeOfValueArray1", "AttributeValue1");
                writer.WriteAttribute("attributeOfValueArray2", "AttributeValue2");
                writer.WriteValue("TestValue3");
                writer.WriteEndValue();
                writer.WriteEndArrayItem();

                writer.WriteEndArray();
                writer.WriteEndElement("valueArray2");

                // Close the previously opened elements
                writer.WriteEndDictElement("secondElement");

                writer.WriteStartElement("thirdElement");
                writer.WriteStartArray();

                writer.WriteStartDictArrayItem();
                writer.WriteValueElement("arrayItem1", 1.0);
                writer.WriteValueElement("arrayItem2", 2.0);
                writer.WriteValueElement("arrayItem3", 3.0);
                writer.WriteEndDictArrayItem();
                writer.WriteStartDictArrayItem();
                writer.WriteValueElement("arrayItem4", 4.0);
                writer.WriteValueElement("arrayItem5", 5.0);
                writer.WriteValueElement("arrayItem6", 6.0);
                writer.WriteEndDictArrayItem();

                writer.WriteEndArray();
                writer.WriteEndElement("thirdElement");

                writer.WriteEndDict();
                writer.WriteEndDocument("rootElement");

                // Output the result
                string result = writer.ToString();
                context.Log.Verify("Output", result);
            }
        }
    }
}
