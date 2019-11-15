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
using Xunit;
using DataCentric;

namespace DataCentric.Test
{
    /// <summary>Unit tests for Double.</summary>
    public class DoubleTest
    {
        /// <summary>Serialization to string.</summary>
        [Fact]
        public void DisplayFormat()
        {
            using (var context = new UnitTestContext(this))
            {
                double baseValue = 1.23456789;
                List<double> values = new List<double>();
                values.Add(0);
                values.Add(0.1);
                values.Add(0.01);
                values.Add(10.0);
                values.Add(100.0);
                values.Add(1000.0);
                values.Add(10000.0);
                values.Add(100000.0);
                for (int i = -8; i < 10; ++i)
                {
                    double value = baseValue * Math.Pow(10, i);
                    values.Add(value);
                }

                // Conversion of positive values to string using default format
                foreach (double value in values)
                {
                    context.Log.Verify($"Positive value to string: {value.DisplayFormat()}");
                }

                // Conversion of negative values to string using default format
                foreach (double value in values)
                {
                    context.Log.Verify($"Negative value to string: {(-value).DisplayFormat()}");
                }
            }
        }
    }
}
