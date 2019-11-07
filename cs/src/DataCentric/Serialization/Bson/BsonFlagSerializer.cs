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
    /// Serializes Flag.Y as true and empty value as null.
    ///
    /// Error message if the value is false.
    /// </summary>
    public class BsonFlagSerializer : SerializerBase<Flag>
    {
        /// <summary>Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.</summary>
        public override Flag Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // Read value as boolean
            bool value = context.Reader.ReadBoolean();

            // The value of false is an error as the only valid values are true and null
            if (!value) throw new Exception("Flag can be serialized as true or null, but not as false. Ensure that Flag variable is nullable.");

            // Return true as the only non-null value
            return Flag.True;
        }

        /// <summary>Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.</summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Flag value)
        {
            // This is to be sure this is a sole-state enum
            if (value != Flag.True) throw new Exception("Flag enum must have a single state. Another state is detected.");

            // Serialize the sole value of enum as true
            context.Writer.WriteBoolean(true);
        }
    }
}
