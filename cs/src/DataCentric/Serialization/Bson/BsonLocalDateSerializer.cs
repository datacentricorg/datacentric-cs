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
    /// Serializes LocalDate as readable int using yyyymmdd format.
    ///
    /// This serializer is used for both the type itself and for its nullable counterpart.
    /// </summary>
    public class BsonLocalDateSerializer : SerializerBase<LocalDate>
    {
        /// <summary>
        /// Deserialize LocalDate from readable int in ISO yyyymmdd format.
        ///
        /// Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.
        /// </summary>
        public override LocalDate Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // LocalDate is serialized as readable int in ISO yyyymmdd format
            int isoDate = context.Reader.ReadInt32();

            // Create LocalDate object by parsing readable int
            var result = LocalDateUtil.FromIsoInt(isoDate);
            return result;
        }

        /// <summary>
        /// Serialize LocalDate to readable int in ISO yyyymmdd format.
        ///
        /// Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.
        /// </summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, LocalDate value)
        {
            // LocalDate is serialized as readable int in ISO yyyymmdd format
            int isoDate = value.ToIsoInt();

            // Serialize as Int32
            context.Writer.WriteInt32(isoDate);
        }
    }
}
