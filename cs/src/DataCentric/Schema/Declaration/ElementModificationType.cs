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
    /// Indicates if the element is treated as input or output during
    /// interactive edit.
    /// </summary>
    public enum ElementModificationType
    {
        /// <summary>Indicates that notification type is not specified.</summary>
        EnumNone = -1,

        /// <summary>
        /// Element is treated as input during interactive edit.
        ///
        /// Input elements cannot be specified by the user. They
        /// will be shown as readonly in the user interface.
        /// </summary>
        In,

        /// <summary>
        /// Element is treated as output during interactive edit.
        ///
        /// Output elements cannot be specified by the user. They
        /// will be shown as readonly in the user interface.
        /// </summary>
        Out
    }
}
