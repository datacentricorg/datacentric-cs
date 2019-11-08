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

namespace DataCentric.Cli
{
    /// <summary>Defines type declaration. A tag of entity type XML representation corresponds to each element of the type.
    /// The names of type elements and corresponding tags coincide.</summary>
    [Serializable]
    [XmlRoot]
    public class TypeDecl : IDecl
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

        /// <summary>Type Params</summary>
        [XmlElement]
        public List<TypeParamDecl> TypeParams { get; set; }

        /// <summary>Type aliases.</summary>
        [XmlElement]
        public List<string> Aliases { get; set; }

        /// <summary>Type kind.</summary>
        [XmlElement]
        public TypeKind? Kind { get; set; }

        public bool IsRecord { get; set; }

        /// <summary>Parent type reference.</summary>
        public TypeDeclKey Inherit { get; set; }

        /// <summary>Inherit Type Argument.</summary>
        [XmlElement]
        public List<TypeArgumentDecl> InheritTypeArguments { get; set; }

        /// <summary>Parent interfaces</summary>
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

        /// <summary>Array of index definitions.</summary>
        [XmlElement]
        public List<TypeIndex> Index { get; set; }

        /// <summary>Immutable flag.</summary>
        public YesNo? Immutable { get; set; }

        /// <summary>Flag indicating UiResponse.</summary>
        public YesNo? UiResponse { get; set; }

        /// <summary>Seed.</summary>
        public int? Seed { get; set; }

        /// <summary>Type Version</summary>
        public string Version { get; set; }

        /// <summary>System.</summary>
        public YesNo? System { get; set; }

        /// <summary>Enable cache flag.</summary>
        public YesNo? EnableCache { get; set; }

        /// <summary>Use IObjectContext Instead of IContext</summary>
        public YesNo? ObjectContext { get; set; }

        /// <summary>Creates object without context</summary>
        public YesNo? ContextFree { get; set; }

        /// <summary>It is possible to split the code over two or more source files.</summary>
        public YesNo? Partial { get; set; }

        /// <summary>Interactive Edit</summary>
        public YesNo? InteractiveEdit { get; set; }

        /// <summary>Save records always permanently.</summary>
        public YesNo? Permanent { get; set; }
    }
}