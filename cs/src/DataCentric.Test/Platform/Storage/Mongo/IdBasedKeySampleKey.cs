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
using DataCentric;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NodaTime;
using Xunit;

namespace DataCentric.Test
{
    /// <summary>
    /// A sample type where the only key element is the record's Id.
    /// </summary>
    [BsonSerializer(typeof(BsonKeySerializer<IdBasedKeySampleKey>))]
    public sealed class IdBasedKeySampleKey : TypedKey<IdBasedKeySampleKey, IdBasedKeySampleData>
    {
        /// <summary>
        /// ObjectId of the record is specific to its version.
        ///
        /// For the record's history to be captured correctly, all
        /// update operations must assign a new ObjectId with the
        /// timestamp that matches update time.
        /// </summary>
        public ObjectId Id { get; set; }
    }
}