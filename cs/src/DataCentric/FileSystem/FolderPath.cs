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
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Reflection;

namespace DataCentric
{
    /// <summary>Utilities for working with dot delimited folder path.</summary>
    public static class FolderPath
    {
        private static char[] dotSeparatorTrimChars = new char[] {' ', '.'};
        private static char[] directorySeparatorTrimChars = new char[] {' ', Path.DirectorySeparatorChar};

        /// <summary>Combines arguments into a single path using dot separator.</summary>
        public static string CombineWithDotSeparator(params string[] paths)
        {
            // Concatenate with dot separator, then removed the trailing separator
            string result = String.Concat(paths.Select(path => WithDotSeparator(path) + "."));
            result = result.TrimEnd('.');
            return result;
        }

        /// <summary>Combines arguments into a single path using directory separator.</summary>
        public static string CombineWithDirectorySeparator(params string[] paths)
        {
            // Concatenate with directory separator, then removed the trailing separator
            string result = String.Concat(paths.Select(path => WithDirectorySeparator(path) + Path.DirectorySeparatorChar));
            result = result.TrimEnd(Path.DirectorySeparatorChar);
            return result;
        }

        /// <summary>Replaces all separators by dot.</summary>
        public static string WithDotSeparator(string path)
        {
            string result = path.Replace(Path.DirectorySeparatorChar, '.')
                .Replace(Path.AltDirectorySeparatorChar, '.')
                .TrimStart(dotSeparatorTrimChars)
                .TrimEnd(dotSeparatorTrimChars);
            return result;
        }

        /// <summary>Replaces all separators and dot by OS specific directory separator.</summary>
        public static string WithDirectorySeparator(string path)
        {
            string result = path.Replace('.', Path.DirectorySeparatorChar)
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .TrimStart(directorySeparatorTrimChars)
                .TrimEnd(directorySeparatorTrimChars);
            return result;
        }
    }
}
