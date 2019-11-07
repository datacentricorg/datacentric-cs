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

namespace DataCentric
{
    /// <summary>Enumeration for tree writer state.</summary>
    public enum TreeWriterState
    {
        /// <summary>Empty</summary>
        Empty,

        /// <summary>State following WriteStartDocument.</summary>
        DocumentStarted,

        /// <summary>State following WriteEndDocument.</summary>
        DocumentCompleted,

        /// <summary>State following WriteStartElement.</summary>
        ElementStarted,

        /// <summary>State following WriteEndElement.</summary>
        ElementCompleted,

        /// <summary>State following WriteStartArray.</summary>
        ArrayStarted,

        /// <summary>State following WriteEndArray.</summary>
        ArrayCompleted,

        /// <summary>State following WriteStartArrayItem.</summary>
        ArrayItemStarted,

        /// <summary>State following WriteEndArrayItem.</summary>
        ArrayItemCompleted,

        /// <summary>State following WriteStartDict that is not inside an array.</summary>
        DictStarted,

        /// <summary>State following WriteEndDict that is not inside an array.</summary>
        DictCompleted,

        /// <summary>State following WriteStartDict inside an array.</summary>
        DictArrayItemStarted,

        /// <summary>State following WriteEndDict inside an array.</summary>
        DictArrayItemCompleted,

        /// <summary>State following WriteStartValue that is not inside an array.</summary>
        ValueStarted,

        /// <summary>State following WriteValue that is not inside an array.</summary>
        ValueWritten,

        /// <summary>State following WriteEndValue that is not inside an array.</summary>
        ValueCompleted,

        /// <summary>State following WriteStartValue inside an array.</summary>
        ValueArrayItemStarted,

        /// <summary>State following WriteValue inside an array.</summary>
        ValueArrayItemWritten,

        /// <summary>State following WriteEndValue inside an array.</summary>
        ValueArrayItemCompleted
    }
}
