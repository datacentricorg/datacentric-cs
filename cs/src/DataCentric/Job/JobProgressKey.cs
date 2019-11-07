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
    /// Information about the job status and progress.
    ///
    /// This record is created with Pending status by the Run() method of the job.
    /// Once the job starts running, it updates its own status as it progresses through
    /// the Running status and ending in one of Completed, Failed, or Cancelled states.
    /// The status may also be updated by the queue to which the job is submitted, e.g.
    /// to record Failed status.
    /// </summary>
    [BsonSerializer(typeof(BsonKeySerializer<JobProgressKey>))]
    public sealed class JobProgressKey : TypedKey<JobProgressKey, JobProgress>
    {
        /// <summary>Monitored job.</summary>
        public JobKey Job { get; set; }
    }
}
