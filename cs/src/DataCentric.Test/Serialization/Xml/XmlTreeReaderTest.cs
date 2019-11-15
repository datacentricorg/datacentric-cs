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
    /// <summary>Unit tests for XmlTreeReader.</summary>
    public class XmlReaderTest
    {
        /// <summary>Test for reading XML with attributes.</summary>
        [Fact]
        public void Attributes()
        {
            using (var context = new UnitTestContext(this))
            {
                string eol = Environment.NewLine;
                string xmlText =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + eol +
                    "<firstElement attributeOfFirstElement1=\"AttributeValue1\" attributeOfFirstElement2=\"AttributeValue2\">" +
                    eol +
                    "  <secondElement>" + eol +
                    "    <valueElement1>TestValue1</valueElement1>" + eol +
                    "    <valueElement2 attributeOfValueElement=\"AttributeValue\">TestValue2</valueElement2>" + eol +
                    "  </secondElement>" + eol +
                    "</firstElement>" + eol;

                // Create XML reader
                ITreeReader reader = new XmlTreeReader(xmlText);

                // Read root element with two attributes
                ITreeReader firstElement = reader.ReadElement("firstElement");
                context.Log.Verify($"attributeOfFirstElement1={firstElement.ReadAttribute("attributeOfFirstElement1")}");
                context.Log.Verify($"attributeOfFirstElement2={firstElement.ReadAttribute("attributeOfFirstElement2")}");

                // Read embedded element
                ITreeReader secondElement = firstElement.ReadElement("secondElement");

                // Read value element using a single method call
                string valueElement1 = secondElement.ReadValueElement("valueElement1");
                context.Log.Verify($"valueElement1={valueElement1}");

                // Add value element with two attributes by creating element explicitly
                ITreeReader valueElementNode2 = secondElement.ReadElement("valueElement2");
                string valueElement2 = valueElementNode2.ReadValue();
                context.Log.Verify($"attributeOfValueElement={valueElementNode2.ReadAttribute("attributeOfValueElement")}");
                context.Log.Verify($"valueElement2={valueElement2}");
            }
        }
    }
}
