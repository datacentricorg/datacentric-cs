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
    /// <summary>Specifies how to open a file for writing.</summary>
    public enum FileWriteModeEnum
    {
        /// <summary>Empty</summary>
        Empty,

        /// <summary>Append to the existing file, or create new file if does not exist.</summary>
        Append,

        /// <summary>Replace (overwrite) the existing file, or create new file if does not exist.</summary>
        Replace,

        /// <summary>Create a new file. Exception if already exists.</summary>
        CreateNew
    }
}
