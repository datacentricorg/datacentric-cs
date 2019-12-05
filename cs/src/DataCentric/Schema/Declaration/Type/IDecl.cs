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
    /// <summary>
    /// Interface for basic declaration types.
    /// </summary>
    public interface IDecl
    {
        /// <summary>Type name is unique when combined with module.</summary>
        string Name { get; set; }

        /// <summary>Module reference.</summary>
        ModuleKey Module { get; set; }

        /// <summary>Category.</summary>
        string Category { get; set; }
    }
}