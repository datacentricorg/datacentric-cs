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

        /// <summary>Type label.</summary>
        public string Label { get; set; }

        /// <summary>Type comment. Contains additional information.</summary>
        public string Comment { get; set; }

        /// <summary>Category.</summary>
        public string Category { get; set; }

        /// <summary>Shortcut.</summary>
        public string Shortcut { get; set; }

        /// <summary>Type aliases.</summary>
        [XmlElement]
        public List<string> Aliases { get; set; }

        /// <summary>Type kind.</summary>
        [XmlElement]
        public TypeKind? Kind { get; set; }

        /// <summary>Indicates if type is derived from Record.</summary>
        public bool IsRecord { get; set; }

        /// <summary>
        /// Reference to the parent type.
        ///
        /// The record can only have a single parent type,
        /// however it can include multiple data interfaces.
        /// </summary>
        public TypeDeclKey Inherit { get; set; }

        /// <summary>Inherit Type Argument.</summary>
        [XmlElement]
        public List<TypeArgumentDecl> InheritTypeArguments { get; set; }

        /// <summary>
        /// List of data interfaces included in this type.
        /// 
        /// In programming languages without multiple class
        /// inheritance, the elements from data interfaces
        /// will be included directly rather than by inheriting
        /// from an interface class.
        /// </summary>
        [XmlElement]
        public List<TypeDeclKey> Interfaces { get; set; }

        /// <summary>Handler declaration block.</summary>
        public HandlerDeclareBlockDecl Declare { get; set; }

        /// <summary>Handler implementation block.</summary>
        public HandlerImplementBlockDecl Implement { get; set; }

        /// <summary>Element declaration block.</summary>
        [XmlElement]
        public List<TypeElementDecl> Elements { get; set; }

        /// <summary>Array of key element names.</summary>
        [XmlElement]
        public List<string> Keys { get; set; }

        /// <summary>
        /// Array of database index definitions, each item representing a single index.
        /// </summary>
        [XmlElement]
        public List<IndexElements> Index { get; set; }

        /// <summary>Immutable flag.</summary>
        public YesNo? Immutable { get; set; }

        /// <summary>Flag indicating if the type will provide UI response.</summary>
        public YesNo? UiResponse { get; set; }

        /// <summary>
        /// Seed used to generate unique hash values for the type.
        ///
        /// This value is used to resolve hash collisions between two types within
        /// the same schema.
        /// </summary>
        public int? Seed { get; set; } // TODO - deprecated

        /// <summary>
        /// Type version is used to make it possible for two classes with
        /// the same name to coexist within the database.
        /// </summary>
        public string Version { get; set; }

        /// <summary>Flag indicating if the type is a system type.</summary>
        public YesNo? System { get; set; }

        /// <summary>Enable cache flag.</summary>
        public YesNo? EnableCache { get; set; }

        /// <summary>Use IObjectContext Instead of Context</summary>
        public YesNo? ObjectContext { get; set; } // TODO - deprecated?

        /// <summary>Creates object without context.</summary>
        public YesNo? ContextFree { get; set; } // TODO - deprecated?

        /// <summary>It is possible to split the code over two or more source files.</summary>
        public YesNo? Partial { get; set; } // TODO - deprecated?

        /// <summary>Flag indicating if the type will support interactive editing.</summary>
        public YesNo? InteractiveEdit { get; set; } // TODO - duplicate of UiResponse?

        /// <summary>Flag indicating a record that is always saved permanently.</summary>
        public YesNo? Permanent { get; set; }
    }
}
