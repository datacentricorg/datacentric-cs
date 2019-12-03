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
    /// <summary>Handler implementation block in type declaration.</summary>
    public class HandlerImplementBlockDecl
    {
        /// <summary>Handler implementation data.</summary>
        [XmlElement]
        public List<HandlerImplementDecl> Handlers { get; set; }

        /// <summary>Interface implementation data.</summary>
        [XmlElement]
        public List<InterfaceImplementDecl> Interfaces { get; set; }
    }
}