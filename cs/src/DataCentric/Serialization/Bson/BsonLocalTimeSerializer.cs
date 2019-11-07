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
    /// Serializes LocalTime as readable integer to millisecond precision in hhmmssfff format.
    /// This serializer is used for both the type itself and for its nullable counterpart.
    /// </summary>
    public class BsonLocalTimeSerializer : SerializerBase<LocalTime>
    {
        /// <summary>
        /// Deserialize LocalTime from readable int in ISO hhmmssfff format.
        ///
        /// Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.
        /// </summary>
        public override LocalTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // LocalTime is serialized as readable int in hhmmssfff format
            int isoTime = context.Reader.ReadInt32();

            // Create LocalTime object by parsing readable int
            var result = LocalTimeUtil.FromIsoInt(isoTime);
            return result;
        }

        /// <summary>
        /// Serialize LocalTime to readable int in ISO hhmmssfff format.
        ///
        /// Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.
        /// </summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, LocalTime value)
        {
            // LocalTime is serialized as readable int in hhmmssfff format
            int isoTime = value.ToIsoInt();

            // Serialize as Int32
            context.Writer.WriteInt32(isoTime);
        }
    }
}
