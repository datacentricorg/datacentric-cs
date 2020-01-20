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
    /// Specifies the method of record or dataset versioning.
    ///
    /// Versioning method is a required field for the data source. Its
    /// value can be overridden for specific record types via an attribute.
    /// </summary>
    public enum VersioningMethod
    {
        /// <summary>
        /// Indicates that enum value is not set.
        /// 
        /// In programming languages where enum defaults to the first item when
        /// not set, making Empty the first item prevents unintended assignment
        /// of a meaningful value.
        /// </summary>
        Empty,

        /// <summary>
        /// Records are versioned and reside in datasets that are themselves
        /// versioned. New records and datasets take precedence over their
        /// earlier versions, but the entire history is preserved and can be
        /// accessed using CutoffTime.
        ///
        /// For a given record and dataset key:
        ///
        /// * Records in datasets created later will take precedence over (override)
        ///   records in datasets created earlier, irrespective of the creation time
        ///   of the record.
        /// * Within the same dataset, records created later will take precedence
        ///   over (override) records created earlier.
        /// </summary>
        Temporal,

        /// <summary>
        /// Records are not versioned but reside in datasets that are versioned.
        /// New records and datasets take precedence over their earlier versions, but
        /// history of a record within the same dataset is not preserved.
        ///
        /// For a given record and dataset key:
        ///
        /// * Records in datasets created later will take precedence over (override)
        ///   records in datasets created earlier, irrespective of the creation time
        ///   of the record.
        /// * Within the same dataset, there can be only one version of the record.
        ///   This is enforced via a unique database index.
        /// </summary>
        NonTemporal,

        /// <summary>
        /// Records are not versioned but reside in datasets that are versioned.
        /// New records and datasets take precedence over their earlier versions, but
        /// history of a record within the same dataset is not preserved and only
        /// a single record is permitted for a given dataset import list.
        ///
        /// With this restriction, a single pass query without ordering by the record
        /// or dataset TemporalId be used, resulting in better performance and simpler
        /// query logic compared to NonTemporal versioning method.
        ///
        /// For a given record and dataset key:
        ///
        /// * Within each dataset import list, there is only one version of the record.
        ///   This is enforced by validation on read, as validation on write would
        ///   cause a performance hit.
        /// * As there is only one version of the record per key, precedence rules
        ///   are unnecessary.
        /// </summary>
        NonOverriding,
    }
}
