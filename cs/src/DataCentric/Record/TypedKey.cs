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
using System.Reflection;

namespace DataCentric
{
    /// <summary>
    /// Base class of a foreign key.
    ///
    /// Generic parameters TKey, TRecord represent a variation on
    /// the curiously recurring template pattern (CRTP). They make
    /// it possible to get key type from the record and vice versa.
    ///
    /// Any elements of defined in the class derived from this one
    /// become key tokens. Property Value and method ToString() of
    /// the key consists of key tokens with semicolon delimiter.
    /// </summary>
    public abstract class TypedKey<TKey, TRecord> : Key
        where TKey : TypedKey<TKey, TRecord>, new()
        where TRecord : TypedRecord<TKey, TRecord>
    {
        /// <summary>
        /// Populate key elements by taking them from the matching
        /// elements of the argument record.
        /// </summary>
        public void PopulateFrom(TypedRecord<TKey, TRecord> record)
        {
            // Assign elements of the record to the matching elements
            // of the key. This will also make string representation
            // of the key return the proper value for the record.
            //
            // Get PropertyInfo arrays for TKey and TRecord
            var dataElementInfoDict = DataTypeInfo.GetOrCreate<TRecord>().DataElementDict;
            var keyElementInfoArray = DataTypeInfo.GetOrCreate<TKey>().DataElements;

            // Check that TRecord has the same or greater number of elements
            // as TKey (all elements of TKey must also be present in TRecord)
            if (dataElementInfoDict.Count < keyElementInfoArray.Length) throw new Exception(
                 $"Record type {typeof(TRecord).Name} has fewer elements than key type {typeof(TKey).Name}.");

            // Iterate over the key elements
            foreach (var keyElementInfo in keyElementInfoArray)
            {
                if (!dataElementInfoDict.TryGetValue(keyElementInfo.Name, out var dataElementInfo))
                {
                    throw new Exception(
                        $"Element {keyElementInfo.Name} of key type {typeof(TKey).Name} " +
                        $"is not found in the record type {typeof(TRecord).Name}.");
                }

                if (keyElementInfo.PropertyType != dataElementInfo.PropertyType)
                    throw new Exception(
                        $"Element {typeof(TKey).Name} has type {keyElementInfo.PropertyType.Name} which does not " +
                        $"match the type {dataElementInfo.PropertyType.Name} of the corresponding element in the " +
                        $"record type {typeof(TRecord).Name}.");

                // Read from the record and assign to the key
                object elementValue = dataElementInfo.GetValue(record);
                keyElementInfo.SetValue(this, elementValue);
            }
        }
    }
}