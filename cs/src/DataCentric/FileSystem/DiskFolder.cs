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

namespace DataCentric
{
    /// <summary>
    /// Implements read-write IFolder interface for a conventional filesystem.
    ///
    /// To provide read-only interface, use DiskReadOnlyFolder class instead.
    /// </summary>
    public class DiskFolder : DiskReadOnlyFolder, IFolder
    {
        /// <summary>Flush data to permanent storage.</summary>
        public virtual void Flush()
        {
            // Uncomment except in root class of the hierarchy
            // base.Flush();
        }

        /// <summary>
        /// Creates or opens the specified file for writing using UTF-8 encoding.
        ///
        /// Append vs. overwrite behavior is determined by write mode.
        ///
        /// This method throws an exception if path is null, an invalid path,
        /// a zero-length string, or if the caller does not have sufficient
        /// permissions to write to the specified file.
        /// </summary>
        public TextWriter GetTextWriter(string filePath, FileWriteModeEnum writeMode)
        {
            // Create full path by combining with output folder path
            string fullFilePath = Path.Combine(FolderPath, filePath);
            string fullFolderPath = Path.GetDirectoryName(fullFilePath);

            // Check if directory exists, if not create
            if (!Directory.Exists(fullFolderPath)) Directory.CreateDirectory(fullFolderPath);

            switch (writeMode)
            {
                case FileWriteModeEnum.Append:
                    return new StreamWriter(fullFilePath);
                case FileWriteModeEnum.Replace:
                    if (File.Exists(fullFilePath)) File.Delete(fullFilePath);
                    return new StreamWriter(fullFilePath);
                case FileWriteModeEnum.CreateNew:
                    if (File.Exists(fullFilePath)) Context.Log.Error($"File {fullFilePath} already exists.");
                    return new StreamWriter(fullFilePath);
                default:
                    throw new Exception($"FileMode={writeMode} is not supported.");
            }
        }

        /// <summary>
        /// Deletes the specified file if it exists.
        ///
        /// This method has no effect if the specified file does not exist.
        ///
        /// This method throws an exception if path is null, an invalid path,
        /// a zero-length string, a directory, or if the caller does not have
        /// sufficient permissions to delete the specified file.
        /// </summary>
        public void DeleteFile(string filePath)
        {
            string fullFilePath = Path.Combine(FolderPath, filePath);
            if (File.Exists(fullFilePath)) File.Delete(fullFilePath);
        }
    }
}
