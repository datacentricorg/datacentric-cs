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
    /// Specifies environment group.
    ///
    /// Some API functions are restricted based on the environment group.
    /// </summary>
    public enum EnvType
    {
        /// <summary>Empty</summary>
        Empty,

        /// <summary>
        /// Production environment group.
        ///
        /// This environment group is used for live production data
        /// and has the most restrictions. For example, it
        /// does not allow a database to be deleted (dropped)
        /// through the API call.
        /// </summary>
        PROD,

        /// <summary>
        /// Shared user acceptance testing environment group.
        ///
        /// This environment group is used has some of the restrictions
        /// of the PROD environment group, including the restriction
        /// on deleting (dropping) the database through an API
        /// call.
        /// </summary>
        UAT,

        /// <summary>
        /// Shared development environment group.
        ///
        /// This environment group is shared but is free from most
        /// restrictions.
        /// </summary>
        DEV,

        /// <summary>
        /// Personal environment group of a specific user.
        ///
        /// This environment group is not shared between users and is
        /// free from most restrictions.
        /// </summary>
        USER,

        /// <summary>
        /// Environment type is used for unit testing.
        ///
        /// Databases for the test environment group are routinely
        /// cleared (deleted). They should not be used for any
        /// purpose other than unit tests.
        /// </summary>
        TEST
    }
}
