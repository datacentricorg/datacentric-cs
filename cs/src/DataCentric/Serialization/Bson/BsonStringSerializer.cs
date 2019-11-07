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

namespace DataCentric
{
    /// <summary>
    /// Serializes and deserializes string, converting
    /// empty or whitespace to null on deserialization only.
    /// </summary>
    public class BsonStringSerializer : SerializerBase<string>
    {
        /// <summary>Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.</summary>
        public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            string result = context.Reader.ReadString();

            // If read string is empty or whitespace, set the result to null instead
            if (string.IsNullOrWhiteSpace(result)) result = null;
            return result;
        }

        /// <summary>Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.</summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
        {
            // At this stage it is not possible to back out from writing the value
            // as the name is already written to BSON, so the only choice is to
            // ignore a possible empty string or whitespace. It will instead be
            // ignored on deserialization if this serializer is used.
            context.Writer.WriteString(value);
        }
    }
}
