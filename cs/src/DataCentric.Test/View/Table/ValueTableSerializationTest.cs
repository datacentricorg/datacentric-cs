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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using DataCentric;
using NodaTime;
using Xunit;

namespace DataCentric.Test
{
    /// <summary>Unit tests for ValueTable.</summary>
    public class ValueTableSerializationTest
    {
        /// <summary>Basic serialization test with single value type.</summary>
        [Fact]
        public void Basic()
        {
            using (var context = new UnitTestContext(this))
            {
                TestSerialization(context, new[] { AtomicType.Int }, TableLayout.NoHeaders);
                TestSerialization(context, new[] { AtomicType.Int }, TableLayout.RowHeaders);
                TestSerialization(context, new[] { AtomicType.Int }, TableLayout.ColHeaders);
                TestSerialization(context, new[] { AtomicType.Int }, TableLayout.RowAndColHeaders);
            }
        }

        /// <summary>Serialization test with multiple value types.</summary>
        [Fact]
        public void MultiType()
        {
            using (var context = new UnitTestContext(this))
            {
                var valueTypes = new[]
                {
                    AtomicType.String,
                    AtomicType.Double,
                    AtomicType.Bool,
                    AtomicType.Int,
                    AtomicType.Long,
                    AtomicType.LocalDate,
                    AtomicType.LocalTime,
                    AtomicType.LocalMinute,
                    AtomicType.LocalDateTime,
                    AtomicType.Instant
                };

                TestSerialization(context, valueTypes, TableLayout.NoHeaders);
                TestSerialization(context, valueTypes, TableLayout.RowHeaders);
                TestSerialization(context, valueTypes, TableLayout.ColHeaders);
                TestSerialization(context, valueTypes, TableLayout.RowAndColHeaders);
            }
        }

        /// <summary>Test serialization.</summary>
        private void TestSerialization(IContext context, AtomicType[] valueTypes, TableLayout layout)
        {
            // Create and resize
            int rowCount = 3;
            int colCount = Math.Max(valueTypes.Length, 4);
            var originalTable = new ValueTable();
            originalTable.Resize(layout, rowCount, colCount);
            PopulateHeaders(originalTable);
            PopulateValues(valueTypes, originalTable);

            // Serialize the generated table and save serialized string to file
            string originalNoHeadersString = originalTable.ToString();
            context.Log.Verify($"{layout}", originalNoHeadersString);

            // Deserialize from string back into table
            var parsedNoHeadersTable = new ValueTable();
            parsedNoHeadersTable.ParseCsv(layout, valueTypes, originalNoHeadersString);
            string parsedNoHeadersString = parsedNoHeadersTable.ToString();

            // Compare serialized strings
            Assert.Equal(originalNoHeadersString, parsedNoHeadersString);
        }

        /// <summary>Populate table headers based on the specified layout.</summary>
        private void PopulateHeaders(ValueTable result)
        {
            TableLayout layout = result.Layout;

            // Populate row headers if they are specified by the layout
            if (layout.HasRowHeaders())
            {
                var rowHeaders = new List<string>();
                for (int rowIndex = 0; rowIndex < result.RowCount; rowIndex++)
                {
                    rowHeaders.Add($"Row{rowIndex}");
                }
                result.RowHeaders = rowHeaders.ToArray();
            }

            // Populate column headers if they are specified by the layout
            if (layout.HasColHeaders())
            {
                var colHeaders = new List<string>();
                for (int colIndex = 0; colIndex < result.ColCount; colIndex++)
                {
                    colHeaders.Add($"Col{colIndex}");
                }
                result.ColHeaders = colHeaders.ToArray();
            }

            // Populate corner header if it is specified by the layout
            if (layout.HasCornerHeader())
            {
                result.CornerHeader = "Corner";
            }
        }

        /// <summary>
        /// Populate with values based on the specified array
        /// of value types, repeating the types in cycle.
        /// </summary>
        private void PopulateValues(AtomicType[] valueTypes, ValueTable result)
        {
            // Initial values to populate the data
            int stringValueAsInt = 0;
            bool boolValue = false;
            int intValue = 0;
            long longValue = 0;
            double doubleValue = 0.5;
            LocalDate localDateValue = new LocalDate(2003,5,1);
            LocalTime localTimeValue = new LocalTime(10, 15, 30);
            LocalMinute localMinuteValue = new LocalMinute(10, 15);
            LocalDateTime localDateTimeValue = new LocalDateTime(2003, 5, 1,10, 15, 0);
            Instant instantValue = new LocalDateTime(2003, 5, 1, 10, 15, 0).ToInstant(DateTimeZone.Utc);

            int valueTypeCount = valueTypes.Length;
            for (int rowIndex = 0; rowIndex < result.RowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < result.ColCount; colIndex++)
                {
                    // Repeat value types in cycle
                    var valueType = valueTypes[colIndex % valueTypeCount];
                    switch (valueType)
                    {
                        case AtomicType.String:
                            result[rowIndex, colIndex] = $"Str{stringValueAsInt++}";
                            break;
                        case AtomicType.Double:
                            result[rowIndex, colIndex] = doubleValue++;
                            break;
                        case AtomicType.Bool:
                            result[rowIndex, colIndex] = boolValue;
                            boolValue = !boolValue;
                            break;
                        case AtomicType.Int:
                            result[rowIndex, colIndex] = intValue++;
                            break;
                        case AtomicType.Long:
                            result[rowIndex, colIndex] = longValue++;
                            break;
                        case AtomicType.LocalDate:
                            result[rowIndex, colIndex] = localDateValue;
                            localDateValue = localDateValue.PlusDays(1);
                            break;
                        case AtomicType.LocalTime:
                            result[rowIndex, colIndex] = localTimeValue;
                            localTimeValue = localTimeValue.PlusHours(1);
                            break;
                        case AtomicType.LocalMinute:
                            result[rowIndex, colIndex] = localMinuteValue;
                            localMinuteValue = localMinuteValue.ToLocalTime().PlusHours(1).ToLocalMinute();
                            break;
                        case AtomicType.LocalDateTime:
                            result[rowIndex, colIndex] = localDateTimeValue;
                            localDateTimeValue = localDateTimeValue.PlusDays(2).PlusHours(2);
                            break;
                        case AtomicType.Instant:
                            result[rowIndex, colIndex] = instantValue;
                            instantValue = instantValue; // TODO Fix, uses previous value
                            break;
                        default: throw new Exception($"Value type {valueType} cannot be stored in ValueTable.");
                    }
                }
            }
        }
    }
}
