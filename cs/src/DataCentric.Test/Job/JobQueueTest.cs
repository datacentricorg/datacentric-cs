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
using System.ComponentModel.Design;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Xunit;
using DataCentric;

namespace DataCentric.Test
{
    /// <summary>Unit test for JobQueue.</summary>
    public class JobQueueTest : UnitTest
    {
        public class SampleKey : TypedKey<SampleKey, SampleRecord>
        {
            /// <summary>Sample field</summary>
            public string SampleName { get; set; }
        }

        public class SampleRecord : TypedRecord<SampleKey, SampleRecord>
        {
            /// <summary>Sample field</summary>
            [BsonRequired]
            public string SampleName { get; set; }

            /// <summary>Sample method.</summary>
            public void SampleMethod()
            {
                Context.Log.Verify("SampleMethod Running");
            }
        }

        /// <summary>Test saving and loading back the job queue record using TemporalId based key.</summary>
        [Fact]
        public void Load()
        {
            using (var context = CreateMethodContext())
            {
                // Create data log record and save
                var dataLog = new DataLog();
                dataLog.LogName = "SampleLog";
                context.SaveOne(dataLog);

                // Create queue record and save, then get its id
                var queue = new JobQueue();
                queue.JobQueueName = "SampleQueue";
                queue.Log = dataLog.ToKey();
                context.SaveOne(queue, context.DataSet);
                var queueId = queue.Id;

                // Create sample record
                var sampleRecord = new SampleRecord();
                sampleRecord.SampleName = "SampleName";
                context.SaveOne(sampleRecord);

                // Create job record and save, then get its id
                var job = new Job();
                job.Queue = queue.ToKey();
                job.CollectionName = DataTypeInfo.GetOrCreate<SampleRecord>().GetCollectionName();
                job.RecordId = sampleRecord.Id;
                job.MethodName = "SampleMethod";
                context.SaveOne(job);
                var jobId = job.Id;

                // Load the records back
                var queueKey = queue.ToKey();
                var loadedQueue = context.Load(queueKey);
                var jobKey = new JobKey { Id = jobId };
                var loadedJob = context.Load(jobKey);

                // Check that TemporalId based key works correctly
                Assert.True(loadedJob.Queue.Value == loadedQueue.ToKey().Value);

                // Run the job
                // TODO - incomment when implemented
                // loadedJob.Run();

                context.Log.Verify("Completed");
            }
        }
    }
}
