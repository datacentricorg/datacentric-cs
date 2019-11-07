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
    /// Test complexity level.
    ///
    /// Higher complexity results in more comprehensive testing at
    /// the expect of longer test running times.
    /// </summary>
    public enum TestComplexity
    {
        /// <summary>Empty</summary>
        Empty,

        /// <summary>
        /// Smoke test.
        ///
        /// Smoke test is the fastest test type to execute and is
        /// the default test complexity level. A smoke test should
        /// not take more than a few minutes to execute.
        /// </summary>
        Smoke,

        /// <summary>
        /// Complete test.
        ///
        /// A complete test may take longer to execute than smoke test,
        /// however it should not take as much time as a performance test.
        /// Typically a complete test should take under 1 min to execute.
        /// </summary>
        Complete,

        /// <summary>
        /// Performance test.
        ///
        /// Use this type for testing system performance under realistic
        /// CPU loads and data volumes. The amount of time a performance
        /// test takes to run may be significant, but effort should be
        /// made to ensure it does not exceed 1 hour.
        /// </summary>
        Performance
    }
}
