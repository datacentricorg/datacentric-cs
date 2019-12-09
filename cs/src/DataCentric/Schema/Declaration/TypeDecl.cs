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
using System.Xml.Serialization;

namespace DataCentric
{
    /// <summary>
    /// Language neutral description of a data class.
    /// </summary>
    [Serializable]
    [XmlRoot]
    public class TypeDecl : TypedRecord<TypeDeclKey, TypeDecl>, IDecl
    {
        /// <summary>Module reference.</summary>
        public ModuleKey Module { get; set; }

        /// <summary>Type name is unique when combined with module.</summary>
        public string Name { get; set; }

        /// <summary>
        /// If specified, will be used in the user interface instead of the name.
        /// 
        /// This field has no effect on the API and affects only the user interface.
        /// </summary>
        public string Label { get; set; }

        /// <summary>Detailed description of the type.</summary>
        public string Comment { get; set; }

        /// <summary>
        /// Dot delimited category providing the ability to group the types inside
        /// the module. Typically maps to the folder where source code for the
        /// data type resides.
        ///
        /// This field has no effect on the API and affects only the user interface.
        /// </summary>
        public string Category { get; set; }

        /// <summary>Type kind.</summary>
        [XmlElement]
        public TypeKind? Kind { get; set; }

        /// <summary>
        /// Indicates if type is derived from Record.
        ///
        /// TODO - overlaps with Kind, consolidate?
        /// </summary>
        public bool IsRecord { get; set; }

        /// <summary>
        /// Reference to the parent type.
        ///
        /// The record can only have a single parent type,
        /// however it can include multiple data interfaces.
        /// </summary>
        public TypeDeclKey Inherit { get; set; }

        /// <summary>Handler declaration block.</summary>
        public HandlerDeclareBlock Declare { get; set; }

        /// <summary>Handler implementation block.</summary>
        public HandlerImplementBlock Implement { get; set; }

        /// <summary>
        /// Each item within this list specifies one element (field)
        /// of the current type.
        /// </summary>
        [XmlElement]
        public List<ElementDecl> Elements { get; set; }

        /// <summary>Array of key element names.</summary>
        [XmlElement]
        public List<string> Keys { get; set; }

        /// <summary>
        /// Array of database index definitions, each item representing a single index.
        ///
        /// TODO - make plural when switching from XML to JSON
        /// </summary>
        [XmlElement]
        public List<IndexElements> Index { get; set; }

        /// <summary>
        /// Immutable flag.
        ///
        /// TODO - introduce an attribute to specify this flag in source code.
        /// </summary>
        public YesNo? Immutable { get; set; }

        /// <summary>
        /// Flag indicating a record that is always saved permanently.
        ///
        /// TODO - introduce an attribute to specify this flag in source code.
        /// </summary>
        public YesNo? Permanent { get; set; }
    }
}
