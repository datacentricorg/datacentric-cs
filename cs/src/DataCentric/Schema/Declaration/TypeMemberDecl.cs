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

using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCentric
{
    /// <summary>Type argument declaration.</summary>
    public class TypeMemberDecl : Data
    {
        /// <summary>Parameters specific to the value element.</summary>
        public ValueDecl Value { get; set; }

        /// <summary>
        /// Reference the declaration of enum contained
        /// by the current element.
        /// </summary>
        public EnumDeclKey Enum { get; set; }

        /// <summary>
        /// Reference to declaration of the data type
        /// contained by the current element.
        ///
        /// The referenced type must have TypeKind=Element.
        /// </summary>
        public TypeDeclKey Data { get; set; }

        /// <summary>
        /// Reference to declaration of the data type for
        /// which the key is contained by the current element.
        ///
        /// The referenced type must not have TypeKind=Element.
        /// </summary>
        public TypeDeclKey Key { get; set; }
    }
}
