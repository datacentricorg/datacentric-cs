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
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Data representation of a user interface items.
    /// </summary>
    public class UiItem : Data
    {
        /// <summary>
        /// The type of the layout item.
        /// </summary>
        [BsonRequired]
        public UiItemType Type { get; set; }

        /// <summary>
        /// The height of this item, relative to the other children of its parent in percent.
        /// </summary>
        [BsonRequired]
        public double? Height { get; set; }

        /// <summary>
        /// The width of this item, relative to the other children of its parent in percent.
        /// </summary>
        [BsonRequired]
        public double? Width { get; set; }

        /// <summary>
        /// Flag indicating that the resizer is present between 
        /// child layout items.
        /// </summary>
        [BsonRequired]
        public bool Resizable { get; set; }

        /// <summary>
        /// An array of layout items that will be created as children of this item.
        /// </summary>
        public List<UiItem> Content { get; set; }
    }
}
