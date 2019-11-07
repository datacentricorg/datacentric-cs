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
using System.Reflection;
using Xunit;

namespace DataCentric.Test
{
    public class AssemblyCacheTest
    {
        [Fact]
        public void Add()
        {
            AssemblyCache builder = new AssemblyCache();

            Assembly assembly1 = Assembly.GetExecutingAssembly();
            Assembly assembly2 = typeof(FactAttribute).Assembly;

            builder.AddAssembly(assembly1);
            Assert.Throws<ArgumentNullException>(() => builder.AddAssembly(null));

            builder.AddAssemblies(new Assembly[] { assembly1, assembly2 });
            Assert.Throws<ArgumentNullException>(() => builder.AddAssemblies(null));
            Assert.Throws<ArgumentException>(() => builder.AddAssemblies(new Assembly[] { assembly1, null }));

            builder.AddFile(assembly1.Location);
            Assert.Throws<ArgumentNullException>(() => builder.AddFile(null));
            Assert.Throws<ArgumentException>(() => builder.AddFile(""));
            Assert.Throws<ArgumentException>(() => builder.AddFile(" "));

            builder.AddFiles(new string[] { assembly1.Location, assembly2.Location });
            Assert.Throws<ArgumentNullException>(() => builder.AddFiles(null));
            Assert.Throws<ArgumentException>(() => builder.AddFiles(new string[] { assembly1.Location, null }));
            Assert.Throws<ArgumentException>(() => builder.AddFiles(new string[] { "", assembly1.Location }));
            Assert.Throws<ArgumentException>(() => builder.AddFiles(new string[] { "  ", assembly1.Location }));
        }

        [Fact]
        public void Sequence()
        {
            Assembly assembly1 = Assembly.GetExecutingAssembly();
            Assembly assembly2 = typeof(FactAttribute).Assembly;
            IEnumerable<Assembly> sequence12 = new Assembly[] { assembly1, assembly2 };
            IEnumerable<Assembly> sequence21 = new Assembly[] { assembly2, assembly1 };

            var sequence1 = new AssemblyCache();
            sequence1.AddAssembly(assembly1);
            sequence1.AddAssembly(assembly2);
            sequence1.AddFile(assembly2.Location);
            sequence1.AddFile(assembly1.Location);

            Assert.True(sequence1.SequenceEqual(sequence12));
            Assert.True(sequence1.SequenceEqual(sequence12));

            var sequence2 = new AssemblyCache();
            sequence2.AddAssemblies(sequence21);
            sequence2.AddAssemblies(sequence12);

            Assert.True(sequence2.SequenceEqual(sequence21));
            Assert.True(sequence2.SequenceEqual(sequence21));

            var sequence3 = new AssemblyCache();
            sequence3.AddFile(assembly1.Location);
            sequence3.AddFile(assembly2.Location);
            sequence3.AddAssembly(assembly1);
            sequence3.AddAssembly(assembly2);

            Assert.True(sequence3.SequenceEqual(sequence12));
            Assert.True(sequence3.SequenceEqual(sequence12));

            var sequence4 = new AssemblyCache();
            sequence4.AddFile(assembly2.Location);
            sequence4.AddAssembly(assembly1);
            sequence4.AddFile(assembly1.Location);
            sequence4.AddAssembly(assembly2);

            Assert.True(sequence4.SequenceEqual(sequence21));
            Assert.True(sequence4.SequenceEqual(sequence21));
        }
    }
}
