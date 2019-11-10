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
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// This class enforces strict naming conventions
    /// for database naming. While format of the resulting database
    /// name is specific to data store type, it always consists
    /// of three tokens: EnvType, EnvGroup, and EnvName.
    /// The meaning of EnvGroup and EnvName tokens depends on
    /// the value of EnvType enumeration.
    ///
    /// This record is stored in root dataset.
    /// </summary>
    [BsonSerializer(typeof(BsonKeySerializer<DbNameKey>))]
    public class DbNameKey : TypedKey<DbNameKey, DbName>
    {
        /// <summary>
        /// Environment type enumeration.
        ///
        /// Some API functions are restricted based on the environment type.
        /// </summary>
        public EnvType EnvType { get; set; }

        /// <summary>
        /// The meaning of environment group depends on the environment type.
        ///
        /// * For PROD, UAT, and DEV environment types, environment group
        ///   identifies the endpoint.
        ///
        /// * For USER environment type, environment group is user alias.
        ///
        /// * For TEST environment type, environment group is the name of
        ///   the unit test class (test fixture).
        /// </summary>
        public string EnvGroup { get; set; }

        /// <summary>
        /// The meaning of environment name depends on the environment type.
        ///
        /// * For PROD, UAT, DEV, and USER environment types, it is the
        ///   name of the user environment selected in the client.
        ///
        /// * For TEST environment type, it is the test method name.
        /// </summary>
        public string EnvName { get; set; }
    }
}
