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
using DataCentric;
using Xunit;

namespace DataCentric.Test
{
    public class ParamKeyValueParserTest
    {
        [Fact]
        public void ParsePair()
        {
            Assert.Equal("[key, value]", ParamKeyValueParser.ParsePair("key=value").ToString());
            Assert.Equal("[key, val=ue]", ParamKeyValueParser.ParsePair("key=val=ue").ToString());
            Assert.Equal("[k, v]", ParamKeyValueParser.ParsePair("k=v").ToString());
            Assert.Equal("[k, ]", ParamKeyValueParser.ParsePair("k=").ToString());
            Assert.Equal("[k, v]", ParamKeyValueParser.ParsePair("k,v", ',').ToString());

            Assert.Throws<ArgumentNullException>(() => ParamKeyValueParser.ParsePair(null));
            Assert.Throws<ArgumentException>(() => ParamKeyValueParser.ParsePair(""));
            Assert.Throws<ArgumentException>(() => ParamKeyValueParser.ParsePair("k"));
            Assert.Throws<ArgumentException>(() => ParamKeyValueParser.ParsePair("=v"));
        }

        [Fact]
        public void Parse()
        {
            bool SequenceEqual(IEnumerable<KeyValuePair<string, string>> first, params string[] second)
                => first.Select(p => p.ToString()).SequenceEqual(second);

            Assert.True(SequenceEqual(ParamKeyValueParser.Parse("k1=v1"), "[k1, v1]"));
            Assert.True(SequenceEqual(ParamKeyValueParser.Parse("k1=v1,k2=v2"), "[k1, v1]", "[k2, v2]"));
            Assert.True(SequenceEqual(ParamKeyValueParser.Parse("k1=v1,,k2=v2"), "[k1, v1]", "[k2, v2]"));
            Assert.True(SequenceEqual(ParamKeyValueParser.Parse("k1=v1,k2=v2,k3=v3"), "[k1, v1]", "[k2, v2]", "[k3, v3]"));

            Assert.True(SequenceEqual(ParamKeyValueParser.Parse("key1-value1;key2-value2", ';', '-'), "[key1, value1]", "[key2, value2]"));
        }
    }
}
