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
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace DataCentric
{
    /// <summary>
    /// Base class of records stored in data source.
    /// </summary>
    [BsonDiscriminator("TypedRecord")]
    public abstract class TypedRecord<TKey, TRecord> : Record
        where TKey : TypedKey<TKey, TRecord>, new()
        where TRecord : TypedRecord<TKey, TRecord>
    {
        /// <summary>
        /// String key consists of semicolon delimited primary key elements:
        ///
        /// KeyElement1;KeyElement2
        ///
        /// To avoid serialization format uncertainty, key elements
        /// can have any atomic type except Double.
        /// </summary>
        public override string Key
        {
            get
            {
                // Assign elements of the record to the matching elements
                // of the key. This will also make string representation
                // of the key return the proper value for the record.
                //
                // Get PropertyInfo arrays for TKey and TRecord
                var tokens = new List<string>();
                var rootTypeName = DataTypeInfo.GetOrCreate(this).GetCollectionName();
                var dataElementInfoDict = DataTypeInfo.GetOrCreate(this).DataElementDict;
                var keyElementInfoArray = DataTypeInfo.GetOrCreate<TKey>().DataElements;

                // Error message if key has more element than the root data type
                if (keyElementInfoArray.Length > dataElementInfoDict.Count)
                    throw new Exception(
                        $"Key type {typeof(TKey).Name} has {keyElementInfoArray.Length} elements " +
                        $"which is greater than {dataElementInfoDict.Count} elements in the " +
                        $"corresponding root data type {rootTypeName}.");

                // Iterate over the key elements
                foreach (var keyElementInfo in keyElementInfoArray)
                {
                    if (!dataElementInfoDict.TryGetValue(keyElementInfo.Name, out var dataElementInfo))
                    {
                        throw new Exception(
                            $"Element {keyElementInfo.Name} of key type {typeof(TKey).Name} " +
                            $"is not found in the root data type {rootTypeName}.");
                    }

                    if (keyElementInfo.PropertyType != dataElementInfo.PropertyType)
                        throw new Exception(
                            $"Element {typeof(TKey).Name} has type {keyElementInfo.PropertyType.Name} which does not " +
                            $"match the type {dataElementInfo.PropertyType.Name} of the corresponding element in the " +
                            $"root data type {rootTypeName}.");

                    // Convert key element to string key token.
                    //
                    // Note that string representation of certain
                    // types inside the key is not the same as what
                    // is returned by ToString(). Specifically,
                    // LocalDate, LocalTime, and LocalMinute are represented
                    // as readable int without delimiters (yyyymmdd,
                    // hhmmssfff, and hhmm respectively) in the key
                    // but use delimited ISO format (yyyy-mm-dd,
                    // hh:mm:ss.fff, and hh:mm) when serialized using
                    // ToString()
                    var token = DataCentric.Key.GetKeyToken(this, dataElementInfo);
                    tokens.Add(token);
                }

                string result = string.Join(";", tokens);
                return result;
            }
            set
            {
                // Do nothing
            }
        }

        //--- METHODS

        /// <summary>
        /// This conversion method creates a new key, populates key elements of the
        /// created key by the values taken from the record, and then caches the
        /// record inside the key using record.DataSet. The cached value will be
        /// used only for lookup in the same dataset but not for lookup in another
        /// dataset for which the current dataset is an import.
        ///
        /// The purpose of caching is to increase the speed of repeated loading, and
        /// to bypass saving the object to the data store and reading it back when
        /// record A has property that is a key for record B, and both records are
        /// created in-memory without any need to save them to storage.
        /// </summary>
        public TKey ToKey()
        {
            TKey result = new TKey();

            // Assign key elements to match the record
            result.PopulateFrom(this);

            return result;
        }
    }
}
