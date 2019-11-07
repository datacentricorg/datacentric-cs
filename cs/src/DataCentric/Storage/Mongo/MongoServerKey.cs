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
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Provides Mongo server URI.
    ///
    /// Server URI specified here must refer to the entire server, not
    /// an individual database.
    /// </summary>
    [BsonSerializer(typeof(BsonKeySerializer<MongoServerKey>))]
    public class MongoServerKey : TypedKey<MongoServerKey, MongoServer>
    {
        /// <summary>
        /// Mongo server URI.
        ///
        /// Server URI specified here must refer to the entire server, not
        /// an individual database.
        /// </summary>
        public string MongoServerUri { get; set; }

        /// <summary>
        /// Mongo server running on default port of localhost.
        /// </summary>
        public static MongoServerKey Default { get; } = new MongoServerKey() { MongoServerUri = "mongodb://localhost:27017" };
    }
}
