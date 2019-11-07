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
    /// <summary>Default locale settings are based on .NET universal locale.</summary>
    public static class LocaleSettings
    {
        /// <summary>
        /// List separator used in serialization.
        ///
        /// The default based on universal locale is comma.
        ///
        /// Thread safety requires that this property is modified as a whole by
        /// assigning a new instance of List; values should not be appended to
        /// an existing list.
        /// </summary>
        public static char ListSeparator { get; set; } = ',';

        /// <summary>
        /// Quote symbol used in serialization.
        ///
        /// The default based on universal locale is double quote.
        ///
        /// Thread safety requires that this property is modified as a whole by
        /// assigning a new instance of List; values should not be appended to
        /// an existing list.
        /// </summary>
        public static char QuoteSymbol { get; set; } = '\"';
    }
}
