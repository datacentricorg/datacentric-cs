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

namespace DataCentric
{
    /// <summary>Type Param Constraint Type.</summary>
    public enum TypeParamConstraintType
    {
        /// <summary>None value is defined.</summary>
        EnumNone = -1,

        /// <summary>Key value.</summary>
        Key,

        /// <summary>Query value.</summary>
        Query,

        /// <summary>Data value.</summary>
        Data,

        /// <summary>Condition value.</summary>
        Condition
    }
}