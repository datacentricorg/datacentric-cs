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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CommandLine;
using Xunit;

namespace DataCentric.Cli
{
    [Verb("test", HelpText = "Test runner.")]
    public class TestCommand
    {
        [Option('p', "pattern", HelpText = "Pattern to filter tests.", Default = "*")]
        public string TestPattern { get; set; }

        /// <summary>
        /// Corresponds to CLI "test" keyword. Executes specified test.
        /// </summary>
        public void Execute()
        {
            AssemblyCache assemblies = new AssemblyCache();

            Regex filter = TypesExtractor.CreateTypeNameFilter(new[] { TestPattern });

            assemblies.AddFiles(Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll"));

            foreach (Assembly assembly in assemblies)
            {
                // Get types with tests
                HashSet<Type> testClasses = assembly.GetTypes()
                                                    .SelectMany(t => t.GetMethods())
                                                    .Where(m => m.GetCustomAttributes().OfType<FactAttribute>().Any())
                                                    .ToList().Select(m => m.DeclaringType).ToHashSet();

                // Filter tests to run
                IEnumerable<Type> testToRun = testClasses.Where(t => filter == null || filter.IsMatch(t.FullName));
                foreach (Type test in testToRun)
                {
                    TestRunner.Run(assembly.GetName().Name, test.FullName);
                }
            }
        }
    }
}