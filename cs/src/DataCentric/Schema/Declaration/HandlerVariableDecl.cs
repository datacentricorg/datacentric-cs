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

namespace DataCentric
{
    /// <summary>Handler parameter or return variable block.</summary>
    public class HandlerParamDecl : TypeMemberDecl
    {
        /// <summary>Parameter name.</summary>
        public string Name { get; set; }

        /// <summary>Flag indicating variable size array (vector) container.</summary>
        public YesNo? Vector { get; set; }

        /// <summary>Object element declaration.</summary>
        public TypeDeclKey Object { get; set; }

        /// <summary>Flag indicating optional element.</summary>
        public YesNo? Optional { get; set; }

        /// <summary>Flag indicating a hidden element. Hidden elements are visible in API but not in the UI.</summary>
        public YesNo? Hidden { get; set; }

        /// <summary>Parameter label.</summary>
        public string Label { get; set; }

        /// <summary>Parameter comment. Contains addition information about handler parameter.</summary>
        public string Comment { get; set; }
    }
}