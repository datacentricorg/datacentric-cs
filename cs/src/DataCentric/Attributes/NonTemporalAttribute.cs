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
using System.Reflection;

namespace DataCentric
{
    /// <summary>
    /// Identifies record type as NonTemporal.
    ///
    /// This method must be set for the root data type of a collection.
    /// Root data type is the type derived directly from TypedRecord.
    ///
    /// For the data type marked by NonTemporal attribute, the data source
    /// keeps only the latest version of the record irrespective of whether
    /// or not NonTemporal flag is set for the data source and/or dataset.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class NonTemporalAttribute : Attribute
    {
    }
}