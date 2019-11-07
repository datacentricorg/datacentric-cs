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
    /// Base class of records stored in root dataset of the data store.
    ///
    /// This class overrides DataSet property to always return TemporalId.Empty.
    /// </summary>
    public abstract class RootRecord<TKey, TRecord> : TypedRecord<TKey, TRecord>
        where TKey : TypedKey<TKey, TRecord>, new()
        where TRecord : RootRecord<TKey, TRecord>
    {
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

            // For this base type of records stored in root dataset,
            // DataSet element has the value designated for the
            // root dataset: TemporalId.Empty.
            DataSet = TemporalId.Empty;
        }
    }
}
