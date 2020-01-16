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

using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Xunit;

namespace DataCentric.Test
{
    public class SchemaFromDataTest : UnitTest
    {
        [Fact]
        public void GenerateTest()
        {
            using (var context = CreateMethodContext())
            {
                /* TODO - uncomment

                var client = new MongoClient();
                var db = client.GetDatabase("TEST;TestTemporalMongoDataSource;TestMultipleDataSetQuery");
                var coll = db.GetCollection<Record>("BaseSample");

                foreach (Record obj in coll.AsQueryable().Sample(5))
                {
                    context.Log.Verify(obj.Key);
                }
                */
            }
        }
    }
}
