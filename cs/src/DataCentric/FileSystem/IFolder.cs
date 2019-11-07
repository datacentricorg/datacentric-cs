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
using System.IO;
using System.Xml;

namespace DataCentric
{
    /// <summary>
    /// Provides a unified API for read-write access to a conventional
    /// filesystem or an alternative backing store such as S3.
    ///
    /// For provide read-only API, use IReadOnlyFolder interface instead.
    /// </summary>
    public interface IFolder : IReadOnlyFolder
    {
        /// <summary>Flush data to permanent storage.</summary>
        void Flush();

        /// <summary>
        /// Creates or opens the specified file for writing using UTF-8 encoding.
        ///
        /// Append vs. overwrite behavior is determined by write mode.
        ///
        /// This method throws an exception if path is null, an invalid path,
        /// a zero-length string, or if the caller does not have sufficient
        /// permissions to write to the specified file.
        /// </summary>
        TextWriter GetTextWriter(string filePath, FileWriteModeEnum writeMode);

        /// <summary>
        /// Deletes the specified file if it exists, or returns without
        /// effect if the file does not exist.
        ///
        /// Error message if the argument is a directory rather than file.
        ///
        /// This method throws an exception if path is null, an invalid path,
        /// a zero-length string, or if the caller does not have sufficient
        /// permissions to delete the specified file.
        /// </summary>
        void DeleteFile(string filePath);
    }

    /// <summary>Extension methods for IFolder.</summary>
    public static class IFolderExtensions
    {
        /// <summary>Appends text to the specified file, creating it if does not exist.</summary>
        public static void AppendText(this IFolder obj, string filePath, string fileContents)
        {
            obj.SaveText(filePath, fileContents, FileWriteModeEnum.Append);
        }

        /// <summary>Writes text to the specified file, overwriting it if exists and creating it otherwise.</summary>
        public static void WriteText(this IFolder obj, string filePath, string fileContents)
        {
            obj.SaveText(filePath, fileContents, FileWriteModeEnum.Replace);
        }

        /// <summary>Appends or overwrites the specified file depending on write mode.</summary>
        private static void SaveText(this IFolder obj, string filePath, string text, FileWriteModeEnum writeMode)
        {
            TextWriter textWriter = obj.GetTextWriter(filePath, writeMode);
            textWriter.Write(text);
            textWriter.Flush();
        }
    }
}
