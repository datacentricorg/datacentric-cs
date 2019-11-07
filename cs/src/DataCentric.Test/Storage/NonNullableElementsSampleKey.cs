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
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using DataCentric;

namespace DataCentric.Test
{

    /// <summary>Key class that has all of the permitted non-nullable key elements included.</summary>
    [BsonSerializer(typeof(BsonKeySerializer<NonNullableElementsSampleKey>))]
    public sealed class NonNullableElementsSampleKey : TypedKey<NonNullableElementsSampleKey, NonNullableElementsSample>
    {
        /// <summary>Sample element.</summary>
        public string StringToken { get; set; }

        /// <summary>Sample element.</summary>
        public bool BoolToken { get; set; }

        /// <summary>Sample element.</summary>
        public int IntToken { get; set; }

        /// <summary>Sample element.</summary>
        public long LongToken { get; set; }

        /// <summary>Sample element.</summary>
        public LocalDate LocalDateToken { get; set; }

        /// <summary>Sample element.</summary>
        public LocalTime LocalTimeToken { get; set; }

        /// <summary>Sample element.</summary>
        public LocalMinute LocalMinuteToken { get; set; }

        /// <summary>Sample element.</summary>
        public LocalDateTime LocalDateTimeToken { get; set; }

        /// <summary>Sample element.</summary>
        public Instant InstantToken { get; set; }

        /// <summary>Sample element.</summary>
        public SampleEnum EnumToken { get; set; }
    }
}
