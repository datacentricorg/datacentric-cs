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
using NodaTime.TimeZones;

namespace DataCentric
{
    /// <summary>
    /// Serializes LocalDateTime in ISO 8601 format:
    ///
    /// 2003-04-21T11:10:00.000Z
    ///
    /// LocalDateTime values are assumed to be in UTC (Z) timezone
    /// when serialized and deserialized to/from BSON.
    ///
    /// This serializer is used for both the type itself
    /// and for its nullable counterpart.
    /// </summary>
    public class BsonLocalDateTimeSerializer : SerializerBase<LocalDateTime>
    {
        /// <summary>
        /// Deserialize LocalDateTime from readable long in ISO 8601 format
        /// to millisecond precision:
        ///
        /// 20030421110000000
        ///
        /// Local datetime values do not have timezone.
        ///
        /// Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.
        /// </summary>
        public override LocalDateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Local datetime in readable ISO long format
            long isoDateTime = context.Reader.ReadInt64();

            // Create LocalDateTime object by parsing readable long
            var result = LocalDateTimeUtil.FromIsoLong(isoDateTime);
            return result;
        }

        /// <summary>
        /// Serialize LocalDateTime from readable long in ISO 8601 format
        /// to millisecond precision:
        ///
        /// 20030421110000000
        ///
        /// Local datetime values do not have timezone.
        ///
        /// Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.
        /// </summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, LocalDateTime value)
        {
            // LocalDateTime is serialized as readable long in ISO
            // yyyymmddhhmmssfff format to millisecond precision
            long isoDateTime = value.ToIsoLong();

            // Serialize as Int32
            context.Writer.WriteInt64(isoDateTime);
        }
    }
}
