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
    /// <summary>
    /// Specifies custom formatting of the element in the UI that
    /// supersedes the default formatting for the type.
    ///
    /// The format is specified using .NET conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FormatAttribute : Attribute // TODO - review use
    {
        /// <summary>
        /// Create from .NET format string.
        /// </summary>
        public FormatAttribute(string format)
        {
            Format = format;
        }

        /// <summary>
        /// Custom format string for the element in the UI that
        /// supersedes the default formatting for the type.
        ///
        /// The format is specified using .NET conventions.
        /// </summary>
        public string Format { get; private set; }
    }
}