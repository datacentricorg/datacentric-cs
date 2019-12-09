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
    /// <summary>
    /// Identifies type kind.
    ///
    /// Some of the type kinds are mapped to class qualifier such as abstract, final, etc.
    /// It can also identify the type as element (class derived from Data) rather than record
    /// that is derived from Record.
    ///
    /// TODO - convert to individual bool elements so more than one can be specified, because more than one can be set
    /// </summary>
    public enum TypeKind
    {
        /// <summary>
        /// Indicates that enum value is not set.
        /// 
        /// In programming languages where enum defaults to the first item when
        /// not set, making Empty the first item prevents unintended assignment
        /// of a meaningful value.
        /// </summary>
        Empty,

        /// <summary>Final type. No types can inherit from final type.</summary>
        Final,

        /// <summary>Abstract type. Abstract type can only be saved in storage as parent of another type.</summary>
        Abstract,

        /// <summary>Element type. Element type cannot be saved in storage directly or as parent of other type.</summary>
        Element
    }
}