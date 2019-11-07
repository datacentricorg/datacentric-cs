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
using NodaTime;

namespace DataCentric
{
    /// <summary>Extension methods for ICollection.</summary>
    public static class IListExtensions
    {
        /// <summary>Deserialize by reading element from the tree reader.</summary>
        public static void DeserializeFrom(this IList obj, string elementName, ITreeReader reader)
        {
            // Check if the list is null
            if (obj == null) throw new Exception("List passed to DeserializeFrom method for element {elementName} is null.");

            // If the list is not empty, error message because the
            // object must be empty prior to parsing
            if (obj.Count > 0) throw new Exception("List passed to DeserializeFrom method for element {elementName} is not empty.");

            // Get item type from list type using reflection
            Type listType = obj.GetType();
            if (!listType.IsGenericType) throw new Exception(
                $"Type {listType} cannot be serialized because it implements only IList but not IList<T>.");
            Type[] genericParameterTypes = listType.GenericTypeArguments;
            if (genericParameterTypes.Length != 1) throw new Exception(
                $"Generic parameter type list {genericParameterTypes} has more than " +
                $"one element creating an ambiguity for deserialization code.");
            Type itemType = genericParameterTypes[0];

            // Select XML node list with the specified name and iterate over XML nodes
            IEnumerable<ITreeReader> selectedXmlNodes = reader.ReadElements(elementName);
            foreach (ITreeReader selectedXmlNode in selectedXmlNodes)
            {
                if (selectedXmlNode == null)
                {
                    // Add null value
                    obj.Add(null);
                }
                else
                {
                    // Switch on item type
                    if (itemType == typeof(string))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token)) obj.Add(token);
                        else obj.Add(null);
                    }
                    else if (itemType == typeof(double) || itemType == typeof(double?))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = double.Parse(token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else if (itemType == typeof(bool) || itemType == typeof(bool?))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = bool.Parse(token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else if (itemType == typeof(int) || itemType == typeof(int?))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = int.Parse(token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else if (itemType == typeof(long) || itemType == typeof(long?))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = long.Parse(token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else if (itemType == typeof(LocalDate) || itemType == typeof(LocalDate?))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = LocalDateUtil.Parse(token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else if (itemType == typeof(LocalTime) || itemType == typeof(LocalTime?))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = LocalTimeUtil.Parse(token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else if (itemType == typeof(LocalMinute) || itemType == typeof(LocalMinute?))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = LocalMinuteUtil.Parse(token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else if (itemType == typeof(LocalDateTime) || itemType == typeof(LocalDateTime?))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = LocalDateTimeUtil.Parse(token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else if (itemType == typeof(Instant) || itemType == typeof(Instant?))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = InstantUtil.Parse(token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else if (itemType.IsSubclassOf(typeof(Enum)))
                    {
                        string token = selectedXmlNode.ReadValue();
                        if (!string.IsNullOrEmpty(token))
                        {
                            var value = Enum.Parse(itemType, token);
                            obj.Add(value);
                        }
                        else obj.Add(null);
                    }
                    else
                    {
                        // If none of the supported atomic types match, use the activator
                        // to create and empty instance of a complex type and populate it
                        var item = Activator.CreateInstance(itemType);
                        switch(item)
                        {
                            case ICollection collectionItem:
                                throw new Exception($"Deserialization is not supported for element {elementName} " +
                                                    $"which is collection containing another collection.");
                            case Data dataItem:
                                var keyItem = dataItem as Key;
                                if (keyItem != null)
                                {
                                    string token = selectedXmlNode.ReadValue();
                                    if (!string.IsNullOrEmpty(token))
                                    {
                                        // Parse semicolon delimited token to populate key item
                                        keyItem.PopulateFrom(token);
                                        obj.Add(item);
                                    }
                                    else obj.Add(null);
                                }
                                else
                                {
                                    // Deserialize data item
                                    dataItem.DeserializeFrom(selectedXmlNode);
                                    obj.Add(item);
                                }
                                break;
                            default:
                                // Error message if the type does not match any of the value or reference types
                                throw new Exception($"Serialization is not supported for type {itemType}.");
                        }
                    }
                }
            }
        }
    }
}
