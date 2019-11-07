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
    /// <summary>Serializes Key as readable integer using semicolon delimited string.</summary>
    public class BsonKeySerializer<TKey> : SerializerBase<TKey> where TKey : Key, new()
    {
        /// <summary>Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.</summary>
        public override TKey Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Read key as string in semicolon delimited format
            string str = context.Reader.ReadString();

            // Deserialize key from semicolon delimited string
            var key = new TKey();
            key.PopulateFrom(str);
            return key;
        }

        /// <summary>Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.</summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TKey value)
        {
            // Serialize key in semicolon delimited format
            context.Writer.WriteString(value.ToString());
        }
    }
}
