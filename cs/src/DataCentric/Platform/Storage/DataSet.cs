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
using System.Collections.Generic;
using CsvHelper.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Dataset is a concept similar to a folder, applied to data in any
    /// data source including relational or document databases, OData
    /// endpoints, etc.
    ///
    /// Datasets can be stored in other datasets. The dataset where dataset
    /// record is stored is called parent dataset.
    ///
    /// Dataset has an Imports array which provides the list of TemporalIds of
    /// datasets where records are looked up if they are not found in the
    /// current dataset. The specific lookup rules are specific to the data
    /// source type and described in detail in the data source documentation.
    ///
    /// Some data source types do not support Imports. If such data
    /// source is used with a dataset where Imports array is not empty,
    /// an error will be raised.
    ///
    /// The root dataset uses TemporalId.Empty and does not have versions
    /// or its own DataSet record. It is always last in the dataset
    /// lookup sequence. The root dataset cannot have Imports.
    /// </summary>
    public class DataSet : TypedRecord<DataSetKey, DataSet>
    {
        /// <summary>
        /// Unique dataset name.
        /// </summary>
        [BsonRequired]
        public string DataSetName { get; set; }

        /// <summary>
        /// Flag indicating that the dataset is non-temporal even if the
        /// data source supports temporal data.
        ///
        /// For the data stored in datasets where NonTemporal == false, a
        /// temporal data source keeps permanent history of changes to each
        /// record within the dataset, and provides the ability to access
        /// the record as of the specified TemporalId, where TemporalId serves
        /// as a timeline (records created later have greater TemporalId than
        /// records created earlier).
        ///
        /// For the data stored in datasets where NonTemporal == true, the
        /// data source keeps only the latest version of the record. All
        /// child datasets of a non-temporal dataset must also be non-temporal.
        ///
        /// In a non-temporal data source, this flag is ignored as all
        /// datasets in such data source are non-temporal.
        /// </summary>
        public bool? NonTemporal { get; set; }

        /// <summary>
        /// List of datasets where records are looked up if they are
        /// not found in the current dataset.
        ///
        /// The specific lookup rules are specific to the data source
        /// type and described in detail in the data source documentation.
        ///
        /// The parent dataset is not included in the list of Imports by
        /// default and must be included in the list of Imports explicitly.
        /// </summary>
        public List<TemporalId> Imports { get; set; }

        //--- METHODS

        /// <summary>
        /// Set Context property and perform validation of the record's data,
        /// then initialize any fields or properties that depend on that data.
        ///
        /// This method must work when called multiple times for the same instance,
        /// possibly with a different context parameter for each subsequent call.
        ///
        /// All overrides of this method must call base.Init(context) first, then
        /// execute the rest of the code in the override.
        /// </summary>
        public override void Init(IContext context)
        {
            // Initialize base before executing the rest of the code in this method
            base.Init(context);

            if (!DataSetName.HasValue())
                throw new Exception(
                    $"DataSetName must be set before Init(context) " +
                    $"method of the dataset is called.");

            if (DataSetName == DataSetKey.Common.DataSetName && DataSet != TemporalId.Empty)
                throw new Exception(
                    $"By convention, Common dataset must be stored in root dataset. " +
                    $"Other datasets may be stored inside any dataset including " +
                    $"the root dataset, Common dataset, or another dataset.");

            if (Imports != null && Imports.Count > 0)
            {
                foreach (var importDataSet in Imports)
                {
                    if (Id <= importDataSet)
                    {
                        if (Id == importDataSet)
                        {
                            throw new Exception(
                                $"Dataset {DataSetName} has an import with the same TemporalId={importDataSet} " +
                                $"as its own TemporalId. Each TemporalId must be unique.");
                        }
                        else
                        {
                            throw new Exception(
                                $"Dataset {DataSetName} has an import whose TemporalId={importDataSet} is greater " +
                                $"than its own TemporalId={Id}. The TemporalId of each import must be strictly " +
                                $"less than the TemporalId of the dataset itself.");
                        }
                    }
                }
            }
        }
    }
}
