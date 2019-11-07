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

namespace DataCentric
{
    /// <summary>Extension methods for IEnumerable.</summary>
    public static class IEnumerableExtensions
    {
        /// <summary>Creates array at current writer level.</summary>
        public static void SerializeTo(this IEnumerable obj, string elementName, ITreeWriter writer)
        {
            // Write start element tag
            writer.WriteStartArrayElement(elementName);

            // Iterate over sequence elements
            foreach (var item in obj)
            {
                // Write array item start tag
                writer.WriteStartArrayItem();

                // Serialize based on type of the item
                switch (item)
                {
                    case null:
                    case string stringItem:
                    case double doubleItem:
                    case bool boolItem:
                    case int intItem:
                    case long longItem:
                        // Write item as is for these types
                        writer.WriteStartValue();
                        writer.WriteValue(item);
                        writer.WriteEndValue();
                        break;
                    case IEnumerable enumerableItem:
                        throw new Exception($"Serialization is not supported for element {elementName} " +
                                            $"which is collection containing another collection.");
                    case Data dataItem:
                        if (dataItem is Key)
                        {
                            // Write key as serialized delimited string
                            writer.WriteStartValue();
                            writer.WriteValue(dataItem.AsString());
                            writer.WriteEndValue();
                        }
                        else
                        {
                            // Embedded data element
                            dataItem.SerializeTo(writer);
                        }
                        break;
                    default:
                        // Argument type is unsupported, error message
                        throw new Exception(
                            $"Element type {item.GetType()} is not supported for tree serialization.");
                }

                // Write array item end tag
                writer.WriteEndArrayItem();
            }

            // Write matching end element tag
            writer.WriteEndArrayElement(elementName);
        }
    }
}
