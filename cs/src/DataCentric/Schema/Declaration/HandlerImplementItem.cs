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
    /// Represents a single item within handler implementation block.
    /// 
    /// Implementation must be provided for each handler that was declared
    /// in this class or its base.
    /// </summary>
    public class HandlerImplementItem : Data
    {
        /// <summary>Handler name.</summary>
        public string Name { get; set; }

        /// <summary>Programming language in which handler is implemented.</summary>
        public Language Language { get; set; }

        /// <summary>
        /// True if this implementation is an override of the
        /// implementation in base class.
        ///
        /// If this flag is false or not set, and base class
        /// provides implementation of the same handler, an
        /// error message will result.
        /// </summary>
        public YesNo? Override { get; set; }
    }
}
