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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Generic record is used to load any record type without
    /// relying on type information. All of its elements that
    /// are not part of the base Record type are provided via
    /// its Elements property.
    /// </summary>
    public sealed class GenericRecord : Record
    {
        /// <summary>
        /// String key consists of semicolon delimited primary key elements:
        ///
        /// KeyElement1;KeyElement2
        ///
        /// To avoid serialization format uncertainty, key elements
        /// can have any atomic type except Double.
        /// </summary>
        public override string Key { get; set; }

        /// <summary>
        /// All elements of the record that are not part of
        /// base class Record are provided via the Elements
        /// property.
        /// </summary>
        [BsonExtraElements]
        public IDictionary<string, object> Elements { get; set; }
    }
}
