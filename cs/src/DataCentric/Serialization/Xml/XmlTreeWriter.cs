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
using System.Text;
using System.IO;
using System.Xml;
using NodaTime;

namespace DataCentric
{
    /// <summary>Implementation of ITreeWriter for XML documents using C# XmlWriter.</summary>
    public class XmlTreeWriter : ITreeWriter
    {
        private MemoryStream stream_;
        private XmlTextWriter xmlTextWriter_;
        private Stack<(string,TreeWriterState)> elementStack_ = new Stack<(string,TreeWriterState)>();
        private TreeWriterState currentState_;

        /// <summary>Create with empty XML document.</summary>
        public XmlTreeWriter()
        {
            // Create a memory stream
            stream_ = new MemoryStream();

            // Create XML text writer around the memory stream
            // Parameter false means that no UTF-8 byte order mark should be included
            // when XML string is generated. This is done to avoid including the byte
            // order mark twice because it will be added by string serialization
            xmlTextWriter_ = new XmlTextWriter(stream_, new UTF8Encoding(false));
            xmlTextWriter_.Formatting = Formatting.Indented;

            // Writer top tag of the XML document (the tag enclosed in ?)
            xmlTextWriter_.WriteStartDocument();

            //!! TODO Provide dispose behavior to this type to dispose stream and XML writer
        }

        /// <summary>Write start document tags. This method
        /// should be called only once for the entire document.</summary>
        public void WriteStartDocument(string rootElementName)
        {
            // Push state and name into the element stack. Writing the actual start tag occurs inside
            // one of WriteStartDict, WriteStartArrayItem, or WriteStartValue calls.
            elementStack_.Push((rootElementName, currentState_));

            if (currentState_ == TreeWriterState.Empty && elementStack_.Count == 1)
            {
                currentState_ = TreeWriterState.DocumentStarted;
            }
            else
                throw new Exception(
                    $"A call to WriteStartDocument(...) must be the first call to the tree writer.");
        }

        /// <summary>Write end document tag. This method
        /// should be called only once for the entire document.
        /// The root element name passed to this method must match the root element
        /// name passed to the preceding call to WriteStartDocument(...).</summary>
        public void WriteEndDocument(string rootElementName)
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.DictCompleted && elementStack_.Count == 1)
            {
                currentState_ = TreeWriterState.DocumentCompleted;
            }
            else
                throw new Exception(
                    $"A call to WriteEndDocument(...) does not follow  WriteEndElement(...) at at root level.");

            // Pop the outer element name and state from the element stack
            string currentElementName = null;
            (currentElementName, currentState_) = elementStack_.Pop();

            // Check that the current element name matches the specified name. Writing the actual end tag
            // occurs inside one of WriteStartDict, WriteStartArrayItem, or WriteStartValue calls.
            if (rootElementName != currentElementName)
                throw new Exception(
                    $"WriteEndDocument({rootElementName}) follows WriteStartDocument({currentElementName}), root element name mismatch.");
        }

        /// <summary>Write element start tag. Each element may contain
        /// a single dictionary, a single value, or multiple array items.</summary>
        public void WriteStartElement(string elementName)
        {
            // Push state and name into the element stack. Writing the actual start tag occurs inside
            // one of WriteStartDict, WriteStartArrayItem, or WriteStartValue calls.
            elementStack_.Push((elementName, currentState_));

            if (currentState_ == TreeWriterState.DocumentStarted) currentState_ = TreeWriterState.ElementStarted;
            else if (currentState_ == TreeWriterState.ElementCompleted) currentState_ = TreeWriterState.ElementStarted;
            else if (currentState_ == TreeWriterState.DictStarted) currentState_ = TreeWriterState.ElementStarted;
            else if (currentState_ == TreeWriterState.DictArrayItemStarted) currentState_ = TreeWriterState.ElementStarted;
            else
                throw new Exception(
                    $"A call to WriteStartElement(...) must follow WriteStartDocument(...) or WriteEndElement(prevName).");
        }

        /// <summary>Write element end tag. Each element may contain
        /// a single dictionary, a single value, or multiple array items.
        /// The element name passed to this method must match the element name passed
        /// to the matching WriteStartElement(...) call at the same indent level.</summary>
        public void WriteEndElement(string elementName)
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.ElementCompleted;
            else if (currentState_ == TreeWriterState.DictCompleted) currentState_ = TreeWriterState.ElementCompleted;
            else if (currentState_ == TreeWriterState.ValueCompleted) currentState_ = TreeWriterState.ElementCompleted;
            else if (currentState_ == TreeWriterState.ArrayCompleted) currentState_ = TreeWriterState.ElementCompleted;
            else
                throw new Exception(
                    $"A call to WriteEndElement(...) does not follow a matching WriteStartElement(...) at the same indent level.");

            // Pop the outer element name and state from the element stack
            string currentElementName = null;
            (currentElementName, currentState_) = elementStack_.Pop();

            // Check that the current element name matches the specified name. Writing the actual end tag
            // occurs inside one of WriteStartDict, WriteStartArrayItem, or WriteStartValue calls.
            if (elementName != currentElementName)
                throw new Exception(
                    $"WriteEndElement({elementName}) follows WriteStartElement({currentElementName}), element name mismatch.");
        }

        /// <summary>Write dictionary start tag. A call to this method
        /// must follow WriteStartElement(...) or WriteStartArrayItem().</summary>
        public void WriteStartDict()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.DocumentStarted) currentState_ = TreeWriterState.DictStarted;
            else if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.DictStarted;
            else if (currentState_ == TreeWriterState.ArrayItemStarted) currentState_ = TreeWriterState.DictArrayItemStarted;
            else
                throw new Exception(
                    $"A call to WriteStartDict() must follow WriteStartElement(...) or WriteStartArrayItem().");

            // Unlike JSON, for XML inner elements are directly inside the outer element
            xmlTextWriter_.WriteStartElement(elementStack_.Peek().Item1);
        }

        /// <summary>Write dictionary end tag. A call to this method
        /// must be followed by WriteEndElement(...) or WriteEndArrayItem().</summary>
        public void WriteEndDict()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.DictStarted) currentState_ = TreeWriterState.DictCompleted;
            else if (currentState_ == TreeWriterState.DictArrayItemStarted) currentState_ = TreeWriterState.DictArrayItemCompleted;
            else if (currentState_ == TreeWriterState.ElementCompleted) currentState_ = TreeWriterState.Empty;
            else
                throw new Exception(
                    $"A call to WriteEndDict(...) does not follow a matching WriteStartDict(...) at the same indent level.");

            // Unlike JSON, for XML inner elements are directly inside the outer element
            xmlTextWriter_.WriteEndElement();
        }

        /// <summary>Write start tag for an array. A call to this method
        /// must follow WriteStartElement(name).</summary>
        public void WriteStartArray()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.ArrayStarted;
            else
                throw new Exception(
                    $"A call to WriteStartArray() must follow WriteStartElement(...).");
        }

        /// <summary>Write end tag for an array. A call to this method
        /// must be followed by WriteEndElement(name).</summary>
        public void WriteEndArray()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ArrayItemCompleted) currentState_ = TreeWriterState.ArrayCompleted;
            else
                throw new Exception(
                    $"A call to WriteEndArray(...) does not follow WriteEndArrayItem(...).");

            // Unlike JSON, for XML inner array elements are directly inside
            // the outer element which is repeated for each array item. Accordingly
            // no XML tag should be written by WriteStartArrayItem() method.
        }

        /// <summary>Write start tag for an array item. A call to this method
        /// must follow either WriteStartArray() or WriteEndArrayItem().</summary>
        public void WriteStartArrayItem()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ArrayStarted) currentState_ = TreeWriterState.ArrayItemStarted;
            else if (currentState_ == TreeWriterState.ArrayItemCompleted) currentState_ = TreeWriterState.ArrayItemStarted;
            else
                throw new Exception(
                    $"A call to WriteStartArrayItem() must follow WriteStartElement(...) or WriteEndArrayItem().");

            // Unlike JSON, for XML inner array elements are directly inside
            // the outer element which is repeated for each array item. Accordingly
            // no XML tag should be written by WriteStartArrayItem() method.
        }

        /// <summary>Write end tag for an array item. A call to this method
        /// must be followed by either WriteEndArray() or WriteStartArrayItem().</summary>
        public void WriteEndArrayItem()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ArrayItemStarted) currentState_ = TreeWriterState.ArrayItemCompleted;
            else if (currentState_ == TreeWriterState.DictArrayItemCompleted) currentState_ = TreeWriterState.ArrayItemCompleted;
            else if (currentState_ == TreeWriterState.ValueArrayItemCompleted) currentState_ = TreeWriterState.ArrayItemCompleted;
            else
                throw new Exception(
                    $"A call to WriteEndArrayItem(...) does not follow a matching WriteStartArrayItem(...) at the same indent level.");

            // Unlike JSON, for XML inner array elements are directly inside
            // the outer element which is repeated for each array item. Accordingly
            // no XML tag should be written by WriteStartArrayItem() method.
        }

        /// <summary>Write value start tag. A call to this method
        /// must follow WriteStartElement(...) or WriteStartArrayItem().</summary>
        public void WriteStartValue()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.ValueStarted;
            else if (currentState_ == TreeWriterState.ArrayItemStarted) currentState_ = TreeWriterState.ValueArrayItemStarted;
            else
                throw new Exception(
                    $"A call to WriteStartValue() must follow WriteStartElement(...) or WriteStartArrayItem().");

            // Unlike JSON, for XML inner elements are directly inside the outer element
            xmlTextWriter_.WriteStartElement(elementStack_.Peek().Item1);
        }

        /// <summary>Write value end tag. A call to this method
        /// must be followed by WriteEndElement(...) or WriteEndArrayItem().</summary>
        public void WriteEndValue()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ValueWritten) currentState_ = TreeWriterState.ValueCompleted;
            else if (currentState_ == TreeWriterState.ValueArrayItemWritten) currentState_ = TreeWriterState.ValueArrayItemCompleted;
            else
                throw new Exception(
                    $"A call to WriteEndValue(...) does not follow a matching WriteValue(...) at the same indent level.");

            // Unlike JSON, for XML inner elements are directly inside the outer element
            xmlTextWriter_.WriteEndElement();
        }

        /// <summary>Write atomic value. Value type
        /// will be inferred from object.GetType().</summary>
        public void WriteValue(object value)
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ValueStarted) currentState_ = TreeWriterState.ValueWritten;
            else if (currentState_ == TreeWriterState.ValueArrayItemStarted) currentState_ = TreeWriterState.ValueArrayItemWritten;
            else
                throw new Exception(
                    $"A call to WriteEndValue(...) does not follow a matching WriteValue(...) at the same indent level.");

            if (value.IsEmpty())
            {
                // Null or empty value is serialized as empty XML tag,
                // accordingly the method returns without writing a value
                // We should only get her for an array as for dictionaries
                // null values should be skipped
                return;
            }

            // Serialize based on value type
            switch (value)
            {
                case string stringValue:
                    xmlTextWriter_.WriteString(stringValue);
                    break;
                case double doubleValue:
                    xmlTextWriter_.WriteString(doubleValue.AsString());
                    break;
                case bool boolValue:
                    xmlTextWriter_.WriteString(boolValue.AsString());
                    break;
                case int intValue:
                    xmlTextWriter_.WriteString(intValue.AsString());
                    break;
                case long longValue:
                    xmlTextWriter_.WriteString(longValue.AsString());
                    break;
                case LocalDate dateValue:
                    // Serialize LocalDate as ISO int in yyyymmdd format
                    int isoDateInt = dateValue.ToIsoInt();
                    xmlTextWriter_.WriteString(isoDateInt.ToString());
                    break;
                case LocalTime timeValue:
                    // Serialize LocalTime as ISO int in hhmmssfff format
                    int isoTimeInt = timeValue.ToIsoInt();
                    xmlTextWriter_.WriteString(isoTimeInt.ToString());
                    break;
                case LocalDateTime dateTimeValue:
                    // Serialize LocalDateTime as ISO long in yyyymmddhhmmssfff format
                    long isoDateTimeLong = dateTimeValue.ToIsoLong();
                    xmlTextWriter_.WriteString(isoDateTimeLong.ToString());
                    break;
                case Instant instantValue:
                    // Serialize Instant as ISO long in yyyymmddhhmmssfff format
                    long isoInstantLong = instantValue.ToIsoLong();
                    xmlTextWriter_.WriteString(isoInstantLong.ToString());
                    break;
                case Enum enumValue:
                    // Serialize enum as string
                    string enumString = enumValue.AsString();
                    xmlTextWriter_.WriteString(enumString);
                    break;
                case Key keyElement:
                    // Serialize key as semicolon delimited string
                    string semicolonDelimitedKeyString = keyElement.AsString();
                    xmlTextWriter_.WriteString(semicolonDelimitedKeyString);
                    break;
                default:
                    // Argument type is unsupported, error message
                    throw new Exception($"Element type {value.GetType()} is not supported for XML serialization.");
            }
        }

        /// <summary>
        /// Add an attribute to the current element.
        ///
        /// One or multiple call(s) to this method must immediately follow
        /// WriteStartElement(name) before any other calls are made.
        ///
        /// When serializing into a representation format that does not support
        /// attributes, such as JSON, the attribute is written as element with
        /// underscore prefix before its name.
        /// </summary>
        public void WriteAttribute(string attributeName, string attributeValue)
        {
            // Check state transition matrix
            if (currentState_ != TreeWriterState.DictStarted &&
                currentState_ != TreeWriterState.DictArrayItemStarted &&
                currentState_ != TreeWriterState.ValueStarted &&
                currentState_ != TreeWriterState.ValueArrayItemStarted)
            {
                throw new Exception(
                    $"A call to WriteAttribute(...) must immediately follow WriteStartDict() or WriteStartValue().");
            }

            xmlTextWriter_.WriteStartAttribute(attributeName);
            xmlTextWriter_.WriteValue(attributeValue);
            xmlTextWriter_.WriteEndAttribute();
        }

        /// <summary>
        /// Convert to XML string without checking that XML document is complete.
        /// This permits the use of this method to inspect the XML content during creation.
        /// </summary>
        public override string ToString()
        {
            xmlTextWriter_.Flush();
            string result = Encoding.UTF8.GetString(stream_.ToArray()) + Environment.NewLine;
            return result;
        }
    }
}
