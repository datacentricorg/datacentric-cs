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
using NodaTime;

namespace DataCentric
{
    /// <summary>Defines customizations in serialization and delegates the rest to the standard provider.</summary>
    public class BsonSerializationProvider : BsonSerializationProviderBase
    {
        /// <summary>Gets a serializer for a type.</summary>
        public override IBsonSerializer GetSerializer(Type type, IBsonSerializerRegistry serializerRegistry)
        {
            if (type == typeof(string)) return new BsonStringSerializer();
            else if (type == typeof(LocalDate)) return new BsonLocalDateSerializer();
            else if (type == typeof(LocalTime)) return new BsonLocalTimeSerializer();
            else if (type == typeof(LocalMinute)) return new BsonLocalMinuteSerializer();
            else if (type == typeof(LocalDateTime)) return new BsonLocalDateTimeSerializer();
            else if (type == typeof(Instant)) return new BsonInstantSerializer();
            else if (type == typeof(TemporalId)) return new BsonTemporalIdSerializer();
            else if (type == typeof(IsoDayOfWeek)) return new BsonIsoDayOfWeekSerializer();
            else if (type == typeof(Flag)) return new BsonFlagSerializer();
            else return null;
        }
    }
}
