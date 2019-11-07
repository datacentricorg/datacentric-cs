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
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;

namespace DataCentric
{
    /// <summary>
    /// Serializes Instant in ISO 8601 format in UTC:
    ///
    /// 2003-04-21T11:10:00.000Z
    ///
    /// This serializer is used for both the type itself
    /// and for its nullable counterpart.
    /// </summary>
    public class BsonInstantSerializer : SerializerBase<Instant>
    {
        /// <summary>
        /// Deserialize Instant from readable long in ISO 8601 format:
        ///
        /// 2003-04-21T11:10:00.000Z
        ///
        /// All datetime values are assumed to be in UTC timezone
        /// and serialized with suffix Z.
        ///
        /// Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.
        /// </summary>
        public override Instant Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Milliseconds since the Unix epoch
            long unixEpochMillis = context.Reader.ReadDateTime();

            // Create Instant object from milliseconds since the Unix epoch
            var result = Instant.FromUnixTimeMilliseconds(unixEpochMillis);
            return result;
        }

        /// <summary>
        /// Serialize Instant to readable long in ISO 8601 format:
        ///
        /// 2003-04-21T11:10:00.000Z
        ///
        /// All datetime values are assumed to be in UTC timezone
        /// and serialized with suffix Z.
        ///
        /// Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.
        /// </summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Instant value)
        {
            // Convert to milliseconds since the Unix epoch
            long unixEpochMillis = value.ToUnixTimeMilliseconds();
            context.Writer.WriteDateTime(unixEpochMillis);
        }
    }
}
