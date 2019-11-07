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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;

namespace DataCentric
{
    /// <summary>
    /// Serializes TemporalId by converting it to TemporalId.
    /// </summary>
    public class BsonTemporalIdSerializer : SerializerBase<TemporalId>
    {
        /// <summary>
        /// Deserialize TemporalId by creating it from TemporalId.
        ///
        /// The serializer accepts empty value.
        /// </summary>
        public override TemporalId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Convert via byte array
            ObjectId objId = context.Reader.ReadObjectId();
            TemporalId result = new TemporalId(objId.ToByteArray());
            return result;
        }

        /// <summary>
        /// Serialize TemporalId by converting it to TemporalId.
        ///
        /// The serializer accepts empty value.
        /// </summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TemporalId value)
        {
            // Convert via byte array
            ObjectId objId = new ObjectId(value.ToByteArray());
            context.Writer.WriteObjectId(objId);
        }
    }
}
