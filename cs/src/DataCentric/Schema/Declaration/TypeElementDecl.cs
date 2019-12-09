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
    /// <summary>Element block within type declaration.</summary>
    public class TypeElementDecl : TypeMemberDecl
    {
        /// <summary>Element name.</summary>
        public string Name { get; set; }

        /// <summary>
        /// If specified, will be used in the user interface instead of the name.
        /// 
        /// This field has no effect on the API and affects only the user interface.
        /// </summary>
        public string Label { get; set; }

        /// <summary>Detailed description of the element.</summary>
        public string Comment { get; set; }

        /// <summary>
        /// Indicate that the current element is a list of the specified type.
        /// </summary>
        public YesNo? Vector { get; set; }

        /// <summary>
        /// Provides the ability to specify alternate names for the current element
        /// when deserializing the data.
        ///
        /// This feature is used primarily to support backward compatibility for
        /// API changes.
        /// </summary>
        [XmlElement]
        public List<string> Aliases { get; set; }

        /// <summary>
        /// Indicates that the element is optional (elements are required by default).
        /// </summary>
        public YesNo? Optional { get; set; }

        /// <summary>Secure flag.</summary>
        public YesNo? Secure { get; set; }

        /// <summary>Flag indicating filterable element.</summary>
        public YesNo? Filterable { get; set; }

        /// <summary>Flag indicating readonly element.</summary>
        public YesNo? ReadOnly { get; set; }

        /// <summary>
        /// Flag indicating a hidden element.
        ///
        /// Hidden elements are present in the API but hidden in the user interface,
        /// except in developer mode.
        /// </summary>
        public YesNo? Hidden { get; set; }

        /// <summary>
        /// Optional flag indicating if the element is additive. For additive elements,
        /// total column can be shown in the user interface.
        ///
        /// This field has no effect on the API and affects only the user interface.
        /// </summary>
        public YesNo? Additive { get; set; }

        /// <summary>
        /// Provides the ability to group the elements in the user interface.
        ///
        /// This field has no effect on the API and affects only the user interface.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Formatting string for the element applied in the user interface.
        ///
        /// TODO - specify formatting convention and accepted format strings.
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// Flag indicating an output element.
        ///
        /// Output elements will be readonly in the user interface. They can only be populated through the API.
        ///
        /// TODO - this duplicates ModificationType, need to consolidate.
        /// </summary>
        public YesNo? Output { get; set; }

        /// <summary>
        /// Specify the name of the element for which the current element as an alternate.
        ///
        /// In the user interface, only one of the alternate elements can be provided.
        /// The default element to be provided is the one for which alternates are specified,
        /// while the alternates have to be selected explicitly.
        /// </summary>
        public string AlternateOf { get; set; }

        /// <summary>
        /// When specified, this element will ne shown in a separate tab with the specified name.
        ///
        /// TODO - partially overlaps with category, consolidate?
        /// </summary>
        public string Viewer { get; set; }

        /// <summary>
        /// Indicates if the element is treated as input or output during
        /// interactive edit.
        ///
        /// TODO - this duplicates Output, need to consolidate.
        /// </summary>
        public ElementModificationType? ModificationType { get; set; }

        /// <summary>
        /// Flag indicating BsonIgnore attribute.
        ///
        /// TODO - review if we need this
        /// </summary>
        public YesNo? BsonIgnore { get; set; }
    }
}