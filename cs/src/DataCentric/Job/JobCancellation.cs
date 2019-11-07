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
    /// The role of JobCancellation record is to enable graceful termination
    /// of a job from inside MethodName of the job before the job is terminated
    /// by the queue.
    ///
    /// The context object created by the Run() method of the job and passed
    /// to the referenced record must from time to time check for the presence
    /// of the cancellation record and set the flag Cancelled of the Context.
    /// The check should be performed with the frequency that will not affect
    /// performance.
    ///
    /// If MethodName does not exit, it is forcibly terminated by the queue
    /// after a grace period.
    /// </summary>
    public sealed class JobCancellation : TypedRecord<JobCancellationKey, JobCancellation>
    {
        /// <summary>The job to which this JobCancellation record applies.</summary>
        [BsonRequired]
        public JobKey Job { get; set; }
    }
}
