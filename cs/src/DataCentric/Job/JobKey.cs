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
using NodaTime;

namespace DataCentric
{
    /// <summary>
    /// The job executes a method of the specified record using:
    ///
    /// * Name of the collection where the record is stored
    /// * TemporalId of the record
    /// * Name of the method to be executed
    /// * List of parameter names (optional)
    /// * List of serialized parameter values (optional)
    ///
    /// The invoked method must return void.
    ///
    /// A job can execute any public method of a class that returns void.
    /// There is no requirement to mark the method by [HandlerMethod] or
    /// [ViewerMethod] attribute.
    /// 
    /// After a job record is created, it is detected and scheduled for
    /// execution by the queue specified by the record.JobQueue element.
    ///
    /// The queue updates the JobProgress record at least every time its
    /// status changes, and optionally more often to update its progress
    /// fraction and progress message. It also monitors the dataset where
    /// it is running for JobCancellation records and writes log entries
    /// to the log specified by the queue.
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
    [BsonSerializer(typeof(BsonKeySerializer<JobKey>))]
    public sealed class JobKey : TypedKey<JobKey, Job>
    {
        /// <summary>
        /// Defining element Id here includes the record's TemporalId
        /// in its key. Because TemporalId of the record is specific
        /// to its version, this is equivalent to using an auto-
        /// incrementing column as part of the record's primary key
        /// in a relational database.
        ///
        /// For the record's history to be captured correctly, all
        /// update operations must assign a new TemporalId with the
        /// timestamp that matches update time.
        /// </summary>
        public TemporalId Id { get; set; }
    }
}
