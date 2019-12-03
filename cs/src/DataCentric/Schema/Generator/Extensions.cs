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

namespace DataCentric.Schema.Generator
{
    public static class Extensions
    {
        public static bool In<T>(this T value, params T[] list) => list.Contains(value);

        public static TResult Map<TSource, TResult>(this TSource input, Func<TSource, TResult> map) => map(input);

        public static void Iterate<TSource>(this IEnumerable<TSource> input, Action<TSource> map)
        {
            foreach (var i in input) map(i);
        }

        public static string TrimStart(this string input, string suffix)
        {
            return input.StartsWith(suffix)
                       ? input.Substring(suffix.Length, input.Length - suffix.Length)
                       : input;
        }

        public static string TrimEnd(this string input, string suffix)
        {
            var lastIndex = input.LastIndexOf(suffix, StringComparison.InvariantCulture);
            return input.EndsWith(suffix)
                       ? input.Substring(0, lastIndex)
                       : input;
        }

        public static bool HasNonWhiteSpaceValue(this string input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }
    }
}