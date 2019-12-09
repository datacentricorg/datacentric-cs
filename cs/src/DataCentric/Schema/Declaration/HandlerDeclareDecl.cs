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
    /// <summary>Handler declaration data.</summary>
    public class HandlerDeclareDecl : Data
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
        public List<HandlerParamDecl> Params { get; set; }

        /// <summary>Handler return value.</summary>
        public HandlerVariableDecl Return { get; set; }

        /// <summary>If this flag is set, handler will be static, otherwise it will be non-static.</summary>
        public YesNo? Static { get; set; }

        /// <summary>If this flag is set, handler will be hidden in UI in user mode.</summary>
        public YesNo? Hidden { get; set; }

        /// <summary>Interactive Input</summary>
        public YesNo? InteractiveInput { get; set; }

        /// <summary>Category.</summary>
        public string Category { get; set; }

        /// <summary>Use the flag to specify that a handler is asynchronous.</summary>
        public YesNo? IsAsync { get; set; }
    }
}