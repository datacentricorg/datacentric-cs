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

namespace DataCentric
{
    /// <summary>
    /// Use nullable variable of this type to represent two-state boolean
    /// when false is not an appropriate default/not set value.
    ///
    /// Non-nullable boolean has two states (true and false) with false
    /// being the default. Use nullable Flag variable in those cases when
    /// null rather than false is a more appropriate default for a two-state
    /// boolean variable.
    ///
    /// When using nullable Flag, the default value is null and the other
    /// value is true.
    /// </summary>
    public enum Flag
    {
        /// <summary>
        /// The sole value of this enum representing true.
        ///
        /// The null value of nullable Flag represents false.
        /// </summary>
        True
    }
}
