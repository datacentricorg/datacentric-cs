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
using NodaTime;

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
    public sealed class JobProgress : TypedRecord<JobProgressKey, JobProgress>
    {
        /// <summary>Monitored job.</summary>
        [BsonRequired]
        public JobKey Job { get; set; }

        /// <summary>
        /// This record is created with Pending status by the Run() method of the job.
        /// Once the job starts running, it updates its own status as it progresses through
        /// the Running status and ending in one of Completed, Failed, or Cancelled states.
        /// The status may also be updated by the queue to which the job is submitted, e.g.
        /// to record Failed status.
        /// </summary>
        [BsonRequired]
        public JobStatus? Status { get; set; }

        /// <summary>
        /// Job progress fraction from 0 to 1.
        ///
        /// The progress fraction is written by the job
        /// to the execution Context. The value from
        /// the Context is then read by the queue running
        /// the job to create this record.
        /// </summary>
        public double? ProgressFraction { get; set; }

        /// <summary>
        /// Message providing information about the current progress,
        /// or an error message if the job fails with an error.
        ///
        /// This is a transient record of the current progress
        /// message and is not the same as the job log. It does
        /// not include the complete logging information, and
        /// not all of the updates to the progress message will
        /// be recorded if they occur in rapid sequence.
        ///
        /// The progress message is written by the job
        /// to the execution Context. The value from
        /// the Context is then read by the queue running
        /// the job to create this record.
        /// </summary>
        public string ProgressMessage { get; set; }
    }
}
