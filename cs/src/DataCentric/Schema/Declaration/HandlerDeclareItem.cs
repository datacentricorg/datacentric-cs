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
    /// <summary>
    /// Represents a single item within handler declaration block.
    ///
    /// Every declared handler must be implemented in this
    /// class or its non-abstract descendant, error message
    /// otherwise.
    /// </summary>
    public class HandlerDeclareItem : Data
    {
        /// <summary>Handler name.</summary>
        public string Name { get; set; }

        /// <summary>Handler label.</summary>
        public string Label { get; set; }

        /// <summary>Handler comment.</summary>
        public string Comment { get; set; }

        /// <summary>Handler type.</summary>
        public HandlerType? Type { get; set; }

        /// <summary>Handler parameters.</summary>
        [XmlElement]
        public List<ParamDecl> Params { get; set; }

        /// <summary>If this flag is set, handler will be static, otherwise it will be non-static.</summary>
        public YesNo? Static { get; set; }

        /// <summary>
        /// If this flag is set, handler will be hidden in the user interface
        /// except in developer mode.
        /// </summary>
        public YesNo? Hidden { get; set; }

        /// <summary>Category.</summary>
        public string Category { get; set; }
    }
}
