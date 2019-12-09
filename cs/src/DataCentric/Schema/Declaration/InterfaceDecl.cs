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
    /// <summary>Defines Interface declaration.</summary>
    public class InterfaceDecl
    {
        /// <summary>Module reference.</summary>
        public ModuleKey Module { get; set; }

        /// <summary>Type name is unique when combined with module.</summary>
        public string Name { get; set; }

        /// <summary>Type label.</summary>
        public string Label { get; set; }

        /// <summary>Type comment. Contains additional information.</summary>
        public string Comment { get; set; }

        /// <summary>Parent interfaces</summary>
        [XmlElement]
        public List<InterfaceDeclKey> Interfaces { get; set; }

        /// <summary>Handler declaration data.</summary>
        [XmlElement]
        public List<HandlerDeclareDecl> Handlers { get; set; }

        /// <summary>Element declaration block.</summary>
        [XmlElement]
        public List<TypeElementDecl> Elements { get; set; }

        /// <summary>Attribute declaration block.</summary>
        [XmlElement]
        public List<TypeElementDecl> Attributes { get; set; }
    }
}