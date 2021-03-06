﻿/*
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
    /// <summary>
    /// Module is a way to organize a group of data types within a package.
    ///
    /// Module name is a dot delimited string which in most cases corresponds
    /// to a code folder.
    /// </summary>
    public class Module : TypedRecord<ModuleKey, Module>
    {
        /// <summary>Unique module name in dot delimited format.</summary>
        public string ModuleName { get; set; }
    }
}