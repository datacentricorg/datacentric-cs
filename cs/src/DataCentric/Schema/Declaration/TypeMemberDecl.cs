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
        /// <summary>Type Param</summary>
        public string TypeParam { get; set; }

        /// <summary>Value or atomic element declaration.</summary>
        public ValueDecl Value { get; set; }

        /// <summary>Enumeration element declaration.</summary>
        public TypeDeclKey Enum { get; set; }

        /// <summary>Data element declaration.</summary>
        public TypeDeclKey Data { get; set; }

        /// <summary>Key element declaration.</summary>
        public TypeDeclKey Key { get; set; }

        /// <summary>Type argument declaration.</summary>
        [XmlElement]
        public List<TypeArgumentDecl> TypeArguments { get; set; }

        /// <summary>Interface element declaration.</summary>
        public TypeDeclKey Interface { get; set; }

        /// <summary>Handler arguments element declaration.</summary>
        public TypeDeclKey HandlerArgs { get; set; }
    }
}