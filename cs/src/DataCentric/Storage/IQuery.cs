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
using System.Linq;
using System.Linq.Expressions;

namespace DataCentric
{
    /// <summary>
    /// Combines methods of IQueryable(TRecord) with additional constraints
    /// and ordering to retrieve the correct version of the record across
    /// multiple datasets.
    /// </summary>
    public interface IQuery<TRecord>
        where TRecord : Record
    {
        /// <summary>
        /// Execution context provides access to key resources including:
        ///
        /// * Logging and error reporting
        /// * Cloud calculation service
        /// * Data sources
        /// * Filesystem
        /// * Progress reporting
        /// </summary>
        IContext Context { get; }

        /// <summary>Filters a sequence of values based on a predicate.</summary>
        IQuery<TRecord> Where(Expression<Func<TRecord, bool>> predicate);

        /// <summary>Sorts the elements of a sequence in ascending order according to the selected key.</summary>
        IQuery<TRecord> SortBy<TProperty>(Expression<Func<TRecord, TProperty>> keySelector);

        /// <summary>Sorts the elements of a sequence in descending order according to the selected key.</summary>
        IQuery<TRecord> SortByDescending<TProperty>(Expression<Func<TRecord, TProperty>> keySelector);

        /// <summary>Convert query to enumerable so iteration can be performed.</summary>
        IEnumerable<TRecord> AsEnumerable();
    }
}
