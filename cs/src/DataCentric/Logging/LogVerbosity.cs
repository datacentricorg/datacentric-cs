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
    /// A log entry is not published to log output if entry verbosity
    /// exceeds log verbosity.
    /// </summary>
    public enum LogVerbosity
    {
        /// <summary>
        /// Error is the default verbosity, used when verbosity is not set.
        ///
        /// Because this is the lowest value in the enumeration, errors are
        /// reported irrespective of the verbosity level.
        /// </summary>
        Error,

        /// <summary>
        /// Warning message.
        ///
        /// Warning messages should be used sparingly to avoid
        /// flooding log output with insignificant warnings.
        /// A warning message should never be generated inside
        /// a loop.
        /// </summary>
        Warning,

        /// <summary>
        /// Info message.
        ///
        /// Info messages should be used sparingly to avoid
        /// flooding log output with superfluous data. An info
        /// message should never be generated inside a loop.
        /// </summary>
        Info,

        /// <summary>Verification message for approval testing.</summary>
        Verify
    }
}
