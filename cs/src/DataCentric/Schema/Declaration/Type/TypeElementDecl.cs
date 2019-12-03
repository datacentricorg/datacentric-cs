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

namespace DataCentric.Schema.Declaration.Type
{
    /// <summary>Type element declaration.</summary>
    public class TypeElementDecl : TypeMemberDecl
    {
        /// <summary>Element name.</summary>
        public string Name { get; set; }

        /// <summary>Element label. If not specified, name is used instead.</summary>
        public string Label { get; set; }

        /// <summary>Element comment. Contains addition information.</summary>
        public string Comment { get; set; }

        /// <summary>Flag indicating variable size array (vector) container.</summary>
        public YesNo? Vector { get; set; }

        /// <summary>Element aliases.</summary>
        [XmlElement]
        public List<string> Aliases { get; set; }

        /// <summary>Flag indicating optional element.</summary>
        public YesNo? Optional { get; set; }

        /// <summary>Secure flag.</summary>
        public YesNo? Secure { get; set; }

        /// <summary>Flag indicating filterable element.</summary>
        public YesNo? Filterable { get; set; }

        /// <summary>Flag indicating readonly element.</summary>
        public YesNo? ReadOnly { get; set; }

        /// <summary>Flag indicating a hidden element. Hidden elements are visible in API but not in the UI.</summary>
        public YesNo? Hidden { get; set; }

        /// <summary>Optional flag indicating if the element is additive and that the total column can be shown in the UI.</summary>
        public YesNo? Additive { get; set; }

        /// <summary>Category.</summary>
        public string Category { get; set; }

        /// <summary>Specifies UI Format for the element.</summary>
        public string Format { get; set; }

        /// <summary>Flag indicating output element. These elements will be readonly in UI and can be fulfilled by handlers.</summary>
        public YesNo? Output { get; set; }

        /// <summary>Link current element to AlternateOf element. In the editor these elements will be treated as a choice.</summary>
        public string AlternateOf { get; set; }

        /// <summary>The element will be viewed under specified tab.</summary>
        public string Viewer { get; set; }

        /// <summary>Element Modification Type.</summary>
        public ElementModificationType? ModificationType { get; set; }

        /// <summary>Flag indicating BsonIgnore attribute.</summary>
        public YesNo? BsonIgnore { get; set; }
    }
}