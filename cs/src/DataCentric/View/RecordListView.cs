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
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// View that displays a table of records stored in collection
    /// with name ViewCollection under TemporalIds specified in
    /// the ViewIds list.
    ///
    /// Collection name is the name of the class at the root of
    /// inheritance hierarchy that is derived directly from
    /// TypedRecord, without namespace.
    /// </summary>
    public sealed class RecordListView : View
    {
        /// <summary>
        /// Type name without namespace of the record displayed as
        /// the current view.
        ///
        /// This record has TemporalId specified by the ViewId field.
        /// </summary>
        [BsonRequired]
        public string ViewCollection { get; set; }

        /// <summary>
        /// TemporalId of the record displayed as the current view.
        ///
        /// This record has type name specified by the TypeName field.
        /// </summary>
        [BsonRequired]
        public List<TemporalId> ViewIds { get; set; }
    }
}
