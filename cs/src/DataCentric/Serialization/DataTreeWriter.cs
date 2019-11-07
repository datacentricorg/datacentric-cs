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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NodaTime;

namespace DataCentric
{
    /// <summary>Implementation of ITreeWriter for Data.</summary>
    public class DataTreeWriter : ITreeWriter
    {
        private struct DataWriterPosition
        {
            public string CurrentElementName { get; set; }
            public TreeWriterState CurrentState { get; set; }
            public Data CurrentDict { get; set; }
            public Dictionary<string, PropertyInfo> CurrentDictElements  { get; set; }
            public PropertyInfo CurrentElementInfo { get; set; }
            public IList CurrentArray { get; set; }
            public Type CurrentArrayItemType { get; set; }
        }

        private Stack<DataWriterPosition> elementStack_ = new Stack<DataWriterPosition>();
        private string rootElementName_;
        private string currentElementName_;
        private TreeWriterState currentState_;
        private Data currentDict_;
        private Dictionary<string, PropertyInfo> currentDictElements_;
        private PropertyInfo currentElementInfo_;
        private IList currentArray_;
        private Type currentArrayItemType_;

        /// <summary>Will write to Data using reflection.</summary>
        public DataTreeWriter(Data data)
        {
            currentDict_ = data;
        }

        /// <summary>
        /// Write start document tags. This method
        /// should be called only once for the entire document.
        /// </summary>
        public void WriteStartDocument(string rootElementName)
        {
            // Check transition matrix
            if (currentState_ == TreeWriterState.Empty && elementStack_.Count == 0) currentState_ = TreeWriterState.DocumentStarted;
            else
                throw new Exception(
                    $"A call to WriteStartDocument(...) must be the first call to the tree writer.");

            // Get root XML element name using mapped final type of the object
            string rootName = currentDict_.GetType().Name;

            // Check that the name matches
            if (rootElementName != rootName) throw new Exception(
                $"Attempting to deserialize data for type {rootElementName} into type {rootName}.");

            rootElementName_ = rootElementName;
            currentElementName_ = rootElementName;
            var currentDictInfoList = DataTypeInfo.GetOrCreate(currentDict_).DataElements;
            currentDictElements_ = new Dictionary<string, PropertyInfo>();
            foreach (var elementInfo in currentDictInfoList) currentDictElements_.Add( elementInfo.Name, elementInfo);
            currentArray_ = null;
            currentArrayItemType_ = null;
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
            if (currentState_ == TreeWriterState.DocumentStarted && elementStack_.Count == 0) currentState_ = TreeWriterState.DocumentCompleted;
            else throw new Exception(
                    $"A call to WriteEndDocument(...) does not follow  WriteEndElement(...) at at root level.");

            // Check that the current element name matches the specified name. Writing the actual end tag
            // occurs inside one of WriteStartDict, WriteStartArrayItem, or WriteStartValue calls.
            if (rootElementName != rootElementName_) throw new Exception(
                $"WriteEndDocument({rootElementName}) follows WriteStartDocument({rootElementName_}), root element name mismatch.");
        }

        /// <summary>
        /// Write element start tag. Each element may contain
        /// a single dictionary, a single value, or multiple array items.
        /// </summary>
        public void WriteStartElement(string elementName)
        {
            if (currentState_ == TreeWriterState.DocumentStarted) currentState_ = TreeWriterState.ElementStarted;
            else if (currentState_ == TreeWriterState.ElementCompleted) currentState_ = TreeWriterState.ElementStarted;
            else if (currentState_ == TreeWriterState.DictStarted) currentState_ = TreeWriterState.ElementStarted;
            else if (currentState_ == TreeWriterState.DictArrayItemStarted) currentState_ = TreeWriterState.ElementStarted;
            else throw new Exception(
                $"A call to WriteStartElement(...) must be the first call or follow WriteEndElement(prevName).");

            currentElementName_ = elementName;
            currentElementInfo_ = currentDictElements_[elementName];
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

            // Check that the current element name matches the specified name. Writing the actual end tag
            // occurs inside one of WriteStartDict, WriteStartArrayItem, or WriteStartValue calls.
            if (elementName != currentElementName_)
                throw new Exception(
                    $"EndComplexElement({elementName}) follows StartComplexElement({currentElementName_}), element name mismatch.");
        }

        /// <summary>
        /// Write dictionary start tag. A call to this method
        /// must follow WriteStartElement(...) or WriteStartArrayItem().
        /// </summary>
        public void WriteStartDict()
        {
            // Push state before defining dictionary state
            PushState();

            // Check state transition matrix
            if (currentState_ == TreeWriterState.DocumentStarted)
            {
                currentState_ = TreeWriterState.DictStarted;

                // Return if this call follows StartDocument, all setup is done in StartDocument
                return;
            }
            else if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.DictStarted;
            else if (currentState_ == TreeWriterState.ArrayItemStarted) currentState_ = TreeWriterState.DictArrayItemStarted;
            else throw new Exception(
                    $"A call to WriteStartDict() must follow WriteStartElement(...) or WriteStartArrayItem().");

            // Set dictionary info
            Type createdDictType = null;
            if (currentArray_ != null) createdDictType = currentArrayItemType_;
            else if (currentDict_ != null) createdDictType = currentElementInfo_.PropertyType;
            else throw new Exception($"Value can only be added to a dictionary or array.");

            object createdDictObj = Activator.CreateInstance(createdDictType);
            if (!(createdDictObj is Data)) // TODO Also support native dictionaries
            {
                string className = currentElementInfo_.PropertyType.Name;
                throw new Exception(
                    $"Element {currentElementInfo_.Name} of type {className} does not implement Data.");
            }

            var createdDict = (Data) createdDictObj;

            // Add to array or dictionary, depending on what we are inside of
            if (currentArray_ != null) currentArray_[currentArray_.Count-1] = createdDict;
            else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, createdDict);
            else throw new Exception($"Value can only be added to a dictionary or array.");

            currentDict_ = (Data) createdDict;
            var currentDictInfoList = DataTypeInfo.GetOrCreate(createdDictType).DataElements;
            currentDictElements_ = new Dictionary<string, PropertyInfo>();
            foreach (var elementInfo in currentDictInfoList) currentDictElements_.Add( elementInfo.Name, elementInfo);
            currentArray_ = null;
            currentArrayItemType_ = null;
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
            else throw new Exception(
                    $"A call to WriteEndDict(...) does not follow a matching WriteStartDict(...) at the same indent level.");

            // Restore previous state
            PopState();
        }

        /// <summary>
        /// Write start tag for an array. A call to this method
        /// must follow WriteStartElement(name).
        /// </summary>
        public void WriteStartArray()
        {
            // Push state
            PushState();

            // Check state transition matrix
            if (currentState_ == TreeWriterState.ElementStarted) currentState_ = TreeWriterState.ArrayStarted;
            else
                throw new Exception(
                    $"A call to WriteStartArray() must follow WriteStartElement(...).");

            // Create the array
            object createdArrayObj = Activator.CreateInstance(currentElementInfo_.PropertyType);
            if (createdArrayObj is IList) // TODO Also support native arrays
            {
                var createdArray = (IList) createdArrayObj;

                // Add to array or dictionary, depending on what we are inside of
                if (currentArray_ != null) currentArray_[currentArray_.Count-1] = createdArray;
                else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, createdArray);
                else throw new Exception($"Value can only be added to a dictionary or array.");

                currentArray_ = createdArray;

                // Get array item type from array type using reflection
                Type listType = currentElementInfo_.PropertyType;
                if (!listType.IsGenericType) throw new Exception(
                    $"Type {listType} cannot be serialized because it implements only IList but not IList<T>.");
                Type[] genericParameterTypes = listType.GenericTypeArguments;
                if (genericParameterTypes.Length != 1) throw new Exception(
                    $"Generic parameter type list {genericParameterTypes} has more than " +
                    $"one element creating an ambiguity for deserialization code.");
                currentArrayItemType_ = genericParameterTypes[0];

                currentDict_ = null;
                currentElementInfo_ = null;
                currentDictElements_ = null;
            }
            else {
                string className = currentElementInfo_.PropertyType.Name;
                throw new Exception(
                    $"Element {currentElementInfo_.Name} of type {className} does not implement ICollection.");
            }
        }

        /// <summary>
        /// Write end tag for an array. A call to this method
        /// must be followed by WriteEndElement(name).
        /// </summary>
        public void WriteEndArray()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ArrayItemCompleted) currentState_ = TreeWriterState.ArrayCompleted;
            else throw new Exception(
                    $"A call to WriteEndArray(...) does not follow WriteEndArrayItem(...).");

            // Pop state
            PopState();
        }

        /// <summary>
        /// Write start tag for an array item. A call to this method
        /// must follow either WriteStartArray(...) or WriteEndArrayItem().
        /// </summary>
        public void WriteStartArrayItem()
        {
            // Check state transition matrix
            if (currentState_ == TreeWriterState.ArrayStarted) currentState_ = TreeWriterState.ArrayItemStarted;
            else if (currentState_ == TreeWriterState.ArrayItemCompleted) currentState_ = TreeWriterState.ArrayItemStarted;
            else throw new Exception(
                    $"A call to WriteStartArrayItem() must follow WriteStartElement(...) or WriteEndArrayItem().");

            object addedItem = null;
            if (currentArrayItemType_ == typeof(string)) addedItem = null;
            else if (currentArrayItemType_ == typeof(double)) addedItem = default(double);
            else if (currentArrayItemType_ == typeof(double?)) addedItem = null;
            else if (currentArrayItemType_ == typeof(bool)) addedItem = default(bool);
            else if (currentArrayItemType_ == typeof(bool?)) addedItem = null;
            else if (currentArrayItemType_ == typeof(int)) addedItem = default(int);
            else if (currentArrayItemType_ == typeof(int?)) addedItem = null;
            else if (currentArrayItemType_ == typeof(long)) addedItem = default(long);
            else if (currentArrayItemType_ == typeof(long?)) addedItem = null;
            else if (currentArrayItemType_ == typeof(LocalDate)) addedItem = default(LocalDate);
            else if (currentArrayItemType_ == typeof(LocalDate?)) addedItem = null;
            else if (currentArrayItemType_ == typeof(LocalTime)) addedItem = default(LocalTime);
            else if (currentArrayItemType_ == typeof(LocalTime?)) addedItem = null;
            else if (currentArrayItemType_ == typeof(LocalMinute)) addedItem = default(LocalMinute);
            else if (currentArrayItemType_ == typeof(LocalMinute?)) addedItem = null;
            else if (currentArrayItemType_ == typeof(LocalDateTime)) addedItem = default(LocalDateTime);
            else if (currentArrayItemType_ == typeof(LocalDateTime?)) addedItem = null;
            else if (currentArrayItemType_ == typeof(Instant)) addedItem = default(Instant);
            else if (currentArrayItemType_ == typeof(Instant?)) addedItem = null;
            else if (currentArrayItemType_.IsClass) addedItem = null;
            else throw new Exception($"Value type {currentArrayItemType_.Name} is not supported for serialization.");

            currentArray_.Add(addedItem);
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
            else throw new Exception(
                    $"A call to WriteEndArrayItem(...) does not follow a matching WriteStartArrayItem(...) at the same indent level.");

            // Do nothing here
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
            else throw new Exception(
                $"A call to WriteStartValue() must follow WriteStartElement(...) or WriteStartArrayItem().");
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
            else throw new Exception(
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
            else throw new Exception(
                    $"A call to WriteEndValue(...) does not follow a matching WriteValue(...) at the same indent level.");

            // Check that we are either inside dictionary or array
            Type elementType = null;
            if (currentArray_ != null) elementType = currentArrayItemType_;
            else if (currentDict_ != null) elementType = currentElementInfo_.PropertyType;
            else throw new Exception($"Cannot WriteValue(...)for element {currentElementName_} " +
                    $"is called outside dictionary or array.");

            if (value.IsEmpty())
            {
                // Do not record null or empty value into dictionary, but add it to an array
                // Add to dictionary or array, depending on what we are inside of
                if (currentArray_ != null) currentArray_[currentArray_.Count-1] = null;
                return;
            }

            // Write based on element type
            Type valueType = value.GetType();
            if (elementType == typeof(string) ||
                elementType == typeof(double) || elementType == typeof(double?) ||
                elementType == typeof(bool) ||  elementType == typeof(bool?) ||
                elementType == typeof(int) ||  elementType == typeof(int?) ||
                elementType == typeof(long) || elementType == typeof(long?))
            {
                // Check type match
                if (!elementType.IsAssignableFrom(valueType))
                    throw new Exception(
                        $"Attempting to deserialize value of type {valueType.Name} " +
                        $"into element of type {elementType.Name}.");

                // Add to array or dictionary, depending on what we are inside of
                if (currentArray_ != null) currentArray_[currentArray_.Count-1] = value;
                else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, value);
                else throw new Exception($"Value can only be added to a dictionary or array.");
            }
            else if (elementType == typeof(LocalDate) || elementType == typeof(LocalDate?))
            {
                // Check type match
                if (valueType != typeof(int))
                    throw new Exception(
                        $"Attempting to deserialize value of type {valueType.Name} " +
                        $"into LocalDate; type should be int32.");

                // Deserialize LocalDate as ISO int in yyyymmdd format
                LocalDate dateValue = LocalDateUtil.FromIsoInt((int)value);

                // Add to array or dictionary, depending on what we are inside of
                if (currentArray_ != null) currentArray_[currentArray_.Count-1] = dateValue;
                else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, dateValue);
                else throw new Exception($"Value can only be added to a dictionary or array.");
            }
            else if (elementType == typeof(LocalTime) || elementType == typeof(LocalTime?))
            {
                // Check type match
                if (valueType != typeof(int))
                    throw new Exception(
                        $"Attempting to deserialize value of type {valueType.Name} " +
                        $"into LocalTime; type should be int32.");

                // Deserialize LocalTime as ISO int in hhmmssfff format
                LocalTime timeValue = LocalTimeUtil.FromIsoInt((int)value);

                // Add to array or dictionary, depending on what we are inside of
                if (currentArray_ != null) currentArray_[currentArray_.Count-1] = timeValue;
                else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, timeValue);
                else throw new Exception($"Value can only be added to a dictionary or array.");
            }
            else if (elementType == typeof(LocalMinute) || elementType == typeof(LocalMinute?))
            {
                // Check type match
                if (valueType != typeof(int))
                    throw new Exception(
                        $"Attempting to deserialize value of type {valueType.Name} " +
                        $"into LocalMinute; type should be int32.");

                // Deserialize LocalTime as ISO int in hhmmssfff format
                LocalMinute minuteValue = LocalMinuteUtil.FromIsoInt((int)value);

                // Add to array or dictionary, depending on what we are inside of
                if (currentArray_ != null) currentArray_[currentArray_.Count - 1] = minuteValue;
                else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, minuteValue);
                else throw new Exception($"Value can only be added to a dictionary or array.");
            }
            else if (elementType == typeof(LocalDateTime) || elementType == typeof(LocalDateTime?))
            {
                // Check type match
                if (valueType != typeof(long))
                    throw new Exception(
                        $"Attempting to deserialize value of type {valueType.Name} " +
                        $"into LocalDateTime; type should be int64.");

                // Deserialize LocalDateTime as ISO long in yyyymmddhhmmssfff format
                LocalDateTime dateTimeValue = LocalDateTimeUtil.FromIsoLong((long)value);

                // Add to array or dictionary, depending on what we are inside of
                if (currentArray_ != null) currentArray_[currentArray_.Count-1] = dateTimeValue;
                else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, dateTimeValue);
                else throw new Exception($"Value can only be added to a dictionary or array.");
            }
            else if (elementType == typeof(Instant) || elementType == typeof(Instant?))
            {
                // Check type match
                if (valueType != typeof(long))
                    throw new Exception(
                        $"Attempting to deserialize value of type {valueType.Name} " +
                        $"into Instant; type should be int64.");

                // Deserialize Instant as ISO long in yyyymmddhhmmssfff format
                Instant instantValue = InstantUtil.FromIsoLong((long)value);

                // Add to array or dictionary, depending on what we are inside of
                if (currentArray_ != null) currentArray_[currentArray_.Count - 1] = instantValue;
                else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, instantValue);
                else throw new Exception($"Value can only be added to a dictionary or array.");
            }
            else if (elementType.IsEnum)
            {
                // Check type match
                if (valueType != typeof(string))
                    throw new Exception(
                        $"Attempting to deserialize value of type {valueType.Name} " +
                        $"into enum {elementType.Name}; type should be string.");

                string stringValue = (string) value;

                // Deserialize enum as string
                string enumString = (string) value;
                object enumValue = Enum.Parse(elementType, enumString);

                // Add to array or dictionary, depending on what we are inside of
                if (currentArray_ != null) currentArray_[currentArray_.Count-1] = enumValue;
                else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, enumValue);
                else throw new Exception($"Value can only be added to a dictionary or array.");
            }
            else
            {
                // We run out of value types at this point, now we can create
                // a reference type and check that it implements Key
                object keyObj = (Key) Activator.CreateInstance(elementType);
                if (keyObj is Key)
                {
                    Key key = (Key) keyObj;

                    // Check type match
                    if (valueType != typeof(string) && valueType != elementType)
                        throw new Exception(
                            $"Attempting to deserialize value of type {valueType.Name} " +
                            $"into key type {elementType.Name}; keys should be serialized into semicolon delimited string.");

                    // Populate by parsing semicolon delimited string
                    string stringValue = value.AsString();
                    key.PopulateFrom(stringValue);

                    // Add to array or dictionary, depending on what we are inside of
                    if (currentArray_ != null) currentArray_[currentArray_.Count-1] = key;
                    else if (currentDict_ != null) currentElementInfo_.SetValue(currentDict_, key);
                    else throw new Exception($"Value can only be added to a dictionary or array.");
                }
                else
                {
                    // Argument type is unsupported, error message
                    throw new Exception($"Element type {value.GetType()} is not supported for serialization.");
                }
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
            if (currentArray_ != null) return currentArray_.AsString();
            else if (currentDict_ != null) return currentDict_.AsString();
            else return GetType().Name;
        }

        /// <summary>Push state to the stack.</summary>
        private void PushState()
        {
            elementStack_.Push(
                new DataWriterPosition()
                {
                    CurrentElementName = currentElementName_,
                    CurrentState = currentState_,
                    CurrentDict = currentDict_,
                    CurrentDictElements = currentDictElements_,
                    CurrentElementInfo = currentElementInfo_,
                    CurrentArray = currentArray_,
                    CurrentArrayItemType = currentArrayItemType_
                });
        }

        /// <summary>Pop state from the stack.</summary>
        private void PopState()
        {
            // Pop the outer element name and state from the element stack
            var stackItem = elementStack_.Pop();
            currentElementName_ = stackItem.CurrentElementName;
            currentState_ = stackItem.CurrentState;
            currentDict_ = stackItem.CurrentDict;
            currentDictElements_ = stackItem.CurrentDictElements;
            currentElementInfo_ = stackItem.CurrentElementInfo;
            currentArray_ = stackItem.CurrentArray;
            currentArrayItemType_ = stackItem.CurrentArrayItemType;
        }
    }
}
