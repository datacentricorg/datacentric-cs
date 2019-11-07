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
using MongoDB.Bson.IO;
using NodaTime;

namespace DataCentric
{
    /// <summary>Implementation of IBsonWriter using MongoDB IBsonWriter.</summary>
    public class BsonTreeWriter : ITreeWriter
    {
        private IBsonWriter bsonWriter_;
        private Stack<(string,TreeWriterState)> elementStack_ = new Stack<(string,TreeWriterState)>();
        private TreeWriterState currentState_;

        /// <summary>Create with empty BSON document.</summary>
        public BsonTreeWriter(IBsonWriter bsonWriter)
        {
            bsonWriter_ = bsonWriter;
        }

        /// <summary>
        /// Write start document tags. This method
        /// should be called only once for the entire document.
        /// </summary>
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

        /// <summary>
        /// Write end document tag. This method
        /// should be called only once for the entire document.
        /// The root element name passed to this method must match the root element
        /// name passed to the preceding call to WriteStartDocument(...).
        /// </summary>
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

        /// <summary>
        /// Write element start tag. Each element may contain
        /// a single dictionary, a single value, or multiple array items.
        /// </summary>
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
                    $"A call to WriteStartElement(...) must be the first call or follow WriteEndElement(prevName).");

            // Write "elementName" :
            bsonWriter_.WriteName(elementStack_.Peek().Item1);
        }

        /// <summary>
        /// Write element end tag. Each element may contain
        /// a single dictionary, a single value, or multiple array items.
        /// The element name passed to this method must match the element name passed
        /// to the matching WriteStartElement(...) call at the same indent level.
        /// </summary>
        public void WriteEndElement(string elementName)
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.ElementCompleted;
            else if (currentState_ == TreeWriterState.DictCompleted) currentState_ = TreeWriterState.ElementCompleted;
            else if (currentState_ == TreeWriterState.ValueCompleted) currentState_ = TreeWriterState.ElementCompleted;
            else if (currentState_ == TreeWriterState.ArrayCompleted) currentState_ = TreeWriterState.ElementCompleted;
            else throw new Exception(
                $"A call to WriteEndElement(...) does not follow a matching WriteStartElement(...) at the same indent level.");

            // Pop the outer element name and state from the element stack
            string currentElementName = null;
            (currentElementName, currentState_) = elementStack_.Pop();

            // Check that the current element name matches the specified name. Writing the actual end tag
            // occurs inside one of WriteStartDict, WriteStartArrayItem, or WriteStartValue calls.
            if (elementName != currentElementName)
                throw new Exception(
                    $"EndComplexElement({elementName}) follows StartComplexElement({currentElementName}), element name mismatch.");

            // Nothing to write here but array closing bracket was written above
        }

        /// <summary>
        /// Write dictionary start tag. A call to this method
        /// must follow WriteStartElement(...) or WriteStartArrayItem().
        /// </summary>
        public void WriteStartDict()
        {
            // Save initial state to be used below
            TreeWriterState prevState = currentState_;

            // Check state transition matrix
            if (currentState_ == TreeWriterState.DocumentStarted) currentState_ = TreeWriterState.DictStarted;
            else if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.DictStarted;
            else if (currentState_ == TreeWriterState.ArrayItemStarted) currentState_ = TreeWriterState.DictArrayItemStarted;
            else
                throw new Exception(
                    $"A call to WriteStartDict() must follow WriteStartElement(...) or WriteStartArrayItem().");

            // Write {
            bsonWriter_.WriteStartDocument();

            // If prev state is DocumentStarted, write _t tag
            if (prevState == TreeWriterState.DocumentStarted)
            {
                string rootElementName = elementStack_.Peek().Item1;
                this.WriteValueElement("_t", rootElementName);
            }
        }

        /// <summary>
        /// Write dictionary end tag. A call to this method
        /// must be followed by WriteEndElement(...) or WriteEndArrayItem().
        /// </summary>
        public void WriteEndDict()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.DictStarted) currentState_ = TreeWriterState.DictCompleted;
            else if (currentState_ == TreeWriterState.DictArrayItemStarted) currentState_ = TreeWriterState.DictArrayItemCompleted;
            else if (currentState_ == TreeWriterState.ElementCompleted) currentState_ = TreeWriterState.DictCompleted;
            else
                throw new Exception(
                    $"A call to WriteEndDict(...) does not follow a matching WriteStartDict(...) at the same indent level.");

            // Write }
            bsonWriter_.WriteEndDocument();
        }

        /// <summary>
        /// Write start tag for an array. A call to this method
        /// must follow WriteStartElement(name).
        /// </summary>
        public void WriteStartArray()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.ArrayStarted;
            else
                throw new Exception(
                    $"A call to WriteStartArray() must follow WriteStartElement(...).");

            // Write [
            bsonWriter_.WriteStartArray();
        }

        /// <summary>
        /// Write end tag for an array. A call to this method
        /// must be followed by WriteEndElement(name).
        /// </summary>
        public void WriteEndArray()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ArrayStarted) currentState_ = TreeWriterState.ArrayCompleted;
            else if (currentState_ == TreeWriterState.ArrayItemCompleted) currentState_ = TreeWriterState.ArrayCompleted;
            else
                throw new Exception(
                    $"A call to WriteEndArray(...) does not follow WriteEndArrayItem(...).");

            // Write ]
            bsonWriter_.WriteEndArray();
        }

        /// <summary>
        /// Write start tag for an array item. A call to this method
        /// must follow either WriteStartArray() or WriteEndArrayItem().
        /// </summary>
        public void WriteStartArrayItem()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ArrayStarted) currentState_ = TreeWriterState.ArrayItemStarted;
            else if (currentState_ == TreeWriterState.ArrayItemCompleted) currentState_ = TreeWriterState.ArrayItemStarted;
            else throw new Exception(
                    $"A call to WriteStartArrayItem() must follow WriteStartElement(...) or WriteEndArrayItem().");

            // Nothing to write here
        }

        /// <summary>
        /// Write end tag for an array item. A call to this method
        /// must be followed by either WriteEndArray() or WriteStartArrayItem().
        /// </summary>
        public void WriteEndArrayItem()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ArrayItemStarted) currentState_ = TreeWriterState.ArrayItemCompleted;
            else if (currentState_ == TreeWriterState.DictArrayItemCompleted) currentState_ = TreeWriterState.ArrayItemCompleted;
            else if (currentState_ == TreeWriterState.ValueArrayItemCompleted) currentState_ = TreeWriterState.ArrayItemCompleted;
            else
                throw new Exception(
                    $"A call to WriteEndArrayItem(...) does not follow a matching WriteStartArrayItem(...) at the same indent level.");

            // Nothing to write here
        }

        /// <summary>
        /// Write value start tag. A call to this method
        /// must follow WriteStartElement(...) or WriteStartArrayItem().
        /// </summary>
        public void WriteStartValue()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.ValueStarted;
            else if (currentState_ == TreeWriterState.ArrayItemStarted) currentState_ = TreeWriterState.ValueArrayItemStarted;
            else
                throw new Exception(
                    $"A call to WriteStartValue() must follow WriteStartElement(...) or WriteStartArrayItem().");

            // Nothing to write here
        }

        /// <summary>
        /// Write value end tag. A call to this method
        /// must be followed by WriteEndElement(...) or WriteEndArrayItem().
        /// </summary>
        public void WriteEndValue()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ValueWritten) currentState_ = TreeWriterState.ValueCompleted;
            else if (currentState_ == TreeWriterState.ValueArrayItemWritten) currentState_ = TreeWriterState.ValueArrayItemCompleted;
            else
                throw new Exception(
                    $"A call to WriteEndValue(...) does not follow a matching WriteValue(...) at the same indent level.");

            // Nothing to write here
        }

        /// <summary>
        /// Write atomic value. Value type
        /// will be inferred from object.GetType().
        /// </summary>
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
                // Null or empty value is serialized as null BSON value.
                // We should only get her for an array as for dictionaries
                // null values should be skipped
                bsonWriter_.WriteNull();
                return;
            }

            // Serialize based on value type
            switch (value)
            {
                case string stringValue:
                    bsonWriter_.WriteString(stringValue);
                    break;
                case double doubleValue:
                    bsonWriter_.WriteDouble(doubleValue);
                    break;
                case bool boolValue:
                    bsonWriter_.WriteBoolean(boolValue);
                    break;
                case int intValue:
                    bsonWriter_.WriteInt32(intValue);
                    break;
                case long longValue:
                    bsonWriter_.WriteInt64(longValue);
                    break;
                case LocalDate dateValue:
                    // Serialize LocalDate as ISO int in yyyymmdd format
                    int isoDateInt = dateValue.ToIsoInt();
                    bsonWriter_.WriteInt32(isoDateInt);
                    break;
                case LocalTime timeValue:
                    // Serialize LocalTime as ISO int in hhmmssfff format
                    int isoTimeInt = timeValue.ToIsoInt();
                    bsonWriter_.WriteInt32(isoTimeInt);
                    break;
                case LocalMinute minuteValue:
                    // Serialize LocalMinute as ISO int in hhmm format
                    int isoMinuteInt = minuteValue.ToIsoInt();
                    bsonWriter_.WriteInt32(isoMinuteInt);
                    break;
                case LocalDateTime dateTimeValue:
                    // Serialize LocalDateTime as ISO long in yyyymmddhhmmssfff format
                    long isoDateTimeLong = dateTimeValue.ToIsoLong();
                    bsonWriter_.WriteInt64(isoDateTimeLong);
                    break;
                case Instant instantValue:
                    // Serialize Instant as ISO long in yyyymmddhhmmssfff format
                    long isoInstantLong = instantValue.ToIsoLong();
                    bsonWriter_.WriteInt64(isoInstantLong);
                    break;
                case Enum enumValue:
                    // Serialize enum as string
                    string enumString = enumValue.AsString();
                    bsonWriter_.WriteString(enumString);
                    break;
                case Key keyElement:
                    // Serialize key as semicolon delimited string
                    string semicolonDelimitedKeyString = keyElement.AsString();
                    bsonWriter_.WriteString(semicolonDelimitedKeyString);
                    break;
                default:
                    // Argument type is unsupported, error message
                    throw new Exception($"Element type {value.GetType()} is not supported for BSON serialization.");
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
            // Because output format does not support native attributes, they are
            // written as elements with underscore prefix before the name
            string elementName = string.Concat("_", attributeName);
            this.WriteValueElement(elementName, attributeValue);
        }

        /// <summary>
        /// Convert to BSON string without checking that BSON document is complete.
        ///
        /// This permits the use of this method to inspect the BSON content during creation.
        /// </summary>
        public override string ToString()
        {
            bsonWriter_.Flush();
            string result = bsonWriter_.ToString();
            return result;
        }
    }
}
