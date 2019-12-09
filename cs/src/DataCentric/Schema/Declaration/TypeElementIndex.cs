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
    /// Identifies a single element inside database index declaration,
    /// for the type, and specifies its direction (ascending or descending).
    /// </summary>
    public class IndexElement : Data
    {
        /// <summary>
        /// Element name.
        ///
        /// Must match one of the element names within the type
        /// declaration, error message otherwise.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specifies direction of the element inside database
        /// index (ascending or descending).
        /// </summary>
        public IndexElementDirection? Direction { get; set; }
    }
}
