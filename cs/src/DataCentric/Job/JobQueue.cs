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
using CsvHelper.Configuration.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// The queue continuously monitors the dataset where it is
    /// running for job records. Once a job record is detected,
    /// the queue schedules the job for execution.
    ///
    /// The queue updates the job status record at least every
    /// time its status changes, and optionally more often to
    /// update its progress fraction and progress message. It
    /// also monitors the dataset where it is running for interrupt
    /// records.
    ///
    /// Because Job records reference the queue by its JobQueueName,
    /// the existing jobs do not need to be resubmitted when a new
    /// queue record is created for the same JobQueueName but it is
    /// important to ensure that only one job with a given JobQueueName
    /// is running at any given time.
    /// 
    /// To run the job, JobQueue executes the Run() method of Job which
    /// in turn invokes method with MethodName in the referenced record
    /// referenced by the job.
    /// 
    /// Depending on the type of queue, MethodName may be executed:
    ///
    /// * In a different process or thread than the one that created the job
    /// * On a different machine than the one where the job was created
    /// * In parallel or out of sequence relative to other jobs
    ///
    /// The job submitter must ensure that the specified method will have
    /// access to the resources it needs and will be able to run successfully
    /// in each of these cases.
    /// </summary>
    public class JobQueue : TypedRecord<JobQueueKey, JobQueue>
    {
        /// <summary>
        /// Unique job queue name.
        ///
        /// Because Job records reference the queue not by its
        /// TemporalId but by its name, existing jobs do not need to
        /// be resubmitted when a new queue record is created.
        /// </summary>
        [BsonRequired]
        public string JobQueueName { get; set; }

        /// <summary>
        /// Log where the job submitted to this queue will write its
        /// output.
        ///
        /// To obtain the entire log, run a query for the Log element of
        /// the entry record, then sort the entry records by their TemporalId.
        /// </summary>
        [BsonRequired]
        public LogKey Log { get; set; }
    }
}
