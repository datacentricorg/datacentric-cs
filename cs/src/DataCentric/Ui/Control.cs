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
using CsvHelper.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Base class of all user interface controls.
    ///
    /// This base type provides a standard way to position the
    /// control within its parent container.
    /// </summary>
    public class Control : UiItem
    {
        /// <summary>
        /// Provides information for positioning the control within its
        /// parent container or parent container's scroll window.
        ///
        /// If position is not specified, the control will be auto
        /// sized and placed at its default location in the flow of
        /// parent container content.
        /// </summary>
        [BsonRequired]
        public Position Position { get; set; }
    }
}
