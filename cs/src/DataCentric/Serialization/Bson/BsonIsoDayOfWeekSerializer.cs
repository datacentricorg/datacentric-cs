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
    /// <summary>Serializes IsoDayOfWeek as string in three-letter format.</summary>
    public class BsonIsoDayOfWeekSerializer : SerializerBase<IsoDayOfWeek>
    {
        /// <summary>Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.</summary>
        public override IsoDayOfWeek Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Read enum as string
            string str = context.Reader.ReadString();

            // Deserialize using the custom method that uses three-letter format
            return IsoDayOfWeekUtil.Parse(str);
        }

        /// <summary>Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.</summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, IsoDayOfWeek value)
        {
            // Extension method AsString() uses three-letter format
            context.Writer.WriteString(value.AsString());
        }
    }
}
