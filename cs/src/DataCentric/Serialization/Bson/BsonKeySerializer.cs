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
            // with collection name followed by the equal sign
            // as prefix, for example:
            //
            // CollectionName=KeyElement1;KeyElement2
            string str = context.Reader.ReadString();

            // Confirm that collection name prefix matches
            // the expected collection name for the type
            // followed by the equal sign (=).
            string[] strTokens = str.Split(new char[] {'='}, 2);
            string collectionName = DataTypeInfo.GetOrCreate(typeof(TKey)).GetCollectionName();
            if (strTokens.Length != 2 || strTokens[0] != collectionName)
                throw new Exception(
                    $"Key {str} does not start from the expected " +
                    $"collection name {collectionName} followed by the equal sign (=).");

            // Deserialize key from the part of the string
            // after the equal sign (=).
            string keyStr = strTokens[1];
            var key = new TKey();
            key.PopulateFrom(keyStr);
            return key;
        }

        /// <summary>Null value is handled via [BsonIgnoreIfNull] attribute and is not expected here.</summary>
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TKey value)
        {
            // Serialize key in semicolon delimited format,
            // with collection name followed by the equal sign
            // as prefix, for example:
            //
            // CollectionName=KeyElement1;KeyElement2
            string collectionName = DataTypeInfo.GetOrCreate(typeof(TKey)).GetCollectionName();
            string keyStr = value.ToString();
            string str = string.Join("=", collectionName, keyStr);
            context.Writer.WriteString(str);
        }
    }
}
