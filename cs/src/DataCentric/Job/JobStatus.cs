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
    /// This enum is used to indicate job's progress from its initial Pending
    /// state through the Running state and ending in one of Completed, Failed,
    /// or Cancelled states.
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// Indicates that value is not set.
        /// </summary>
        Empty,

        /// <summary>
        /// The job has been submitted to the queue but is not yet running.
        ///
        /// This status is created by the Run() method of the job.
        /// </summary>
        Pending,

        /// <summary>
        /// The job is running.
        ///
        /// This status is created by the job itself when it starts running.
        /// </summary>
        Running,

        /// <summary>
        /// The job completed successfully.
        ///
        /// This status is created by the job itself when it exits successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// The job has failed.
        ///
        /// This state is distinct from Cancelled, which is the
        /// end state if the job did not fail on its own but was
        /// cancelled by creating an interrupt record.
        ///
        /// Because there is no guarantee that a job would be able
        /// to record its status in case of a failure, this status
        /// is created by the job itself (if possible), and after
        /// the failure is detected also by the queue running the job.
        /// </summary>
        Failed,

        /// <summary>
        /// The job has been cancelled by creating an  interrupt record.
        ///
        /// After an interrupt record is detected by the queue, the job
        /// is given 10 seconds to shut down gracefully (soft cancellation);
        /// after that time it is terminated by the queue (hard cancellation).
        /// The code should be written defensively to continue without errors
        /// in case of either soft or hard cancellation.
        ///
        /// This status is created by the job itself when it detects an
        /// interrupt with cancellation request.
        /// </summary>
        Cancelled
    }
}
