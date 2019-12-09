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
    /// Identifies the programming language in which a handler is implemented.
    ///
    /// By convention, language name is the same as source file suffix:
    ///
    /// * For Python, py
    /// * For C++, cpp
    /// * For C#, cs
    ///
    /// The language is used to select which DataCentric CLI to invoke to execute
    /// the handler. For example, if language name is py, the CLI to invoke is
    /// datacentric-py. 
    /// </summary>
    public class LanguageKey : Data // TODO - convert to record so key can be picked
    {
        /// <summary>Unique language identifier.</summary>
        public string LanguageName { get; set; }
    }
}