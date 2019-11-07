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
using DataCentric;

namespace DataCentric.Test
{
    /// <summary>Derived type sample for unit testing.</summary>
    public class DerivedTypeSample : BaseTypeSample
    {
        /// <summary>Integer element of derived type.</summary>
        public int? IntElement { get; set; }

        /// <summary>List of nullable integers.</summary>
        public List<int> NonNullableIntList { get; set; }

        /// <summary>List of nullable integers.</summary>
        public List<int?> NullableIntList { get; set; }

        /// <summary>List of strings.</summary>
        public List<string> StringList { get; set; }

        /// <summary>Key element.</summary>
        public BaseTypeSampleKey KeyElement { get; set; }

        /// <summary>List of key elements.</summary>
        public List<BaseTypeSampleKey> KeyList { get; set; }

        /// <summary>Data element.</summary>
        public ElementTypeSample DataElement { get; set; }

        /// <summary>List of data elements.</summary>
        public List<ElementTypeSample> DataList { get; set; }

        /// <summary>Method of derived type.</summary>
        public int GetIntElement()
        {
            return IntElement.Value;
        }
    }
}
