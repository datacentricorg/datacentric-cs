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
    /// Provides a unified API for read-only access to a conventional
    /// filesystem or an alternative backing store such as S3.
    ///
    /// For provide read-write API, use IFolder interface instead.
    /// </summary>
    public interface IReadOnlyFolder : IDisposable
    {
        /// <summary>
        /// Execution context provides access to key resources including:
        ///
        /// * Logging and error reporting
        /// * Cloud calculation service
        /// * Data sources
        /// * Filesystem
        /// * Progress reporting
        /// </summary>
        IContext Context { get; }

        //--- METHODS

        /// <summary>
        /// Set Context property and perform validation of the record's data,
        /// then initialize any fields or properties that depend on that data.
        ///
        /// This method may be called multiple times for the same instance,
        /// possibly with a different context parameter for each subsequent call.
        ///
        /// IMPORTANT - Every override of this method must call base.Init()
        /// first, and only then execute the rest of the override method's code.
        /// </summary>
        void Init(IContext context);

        /// <summary>
        /// True if the caller has the required permissions and path contains
        /// the name of an existing file; otherwise, false.
        ///
        /// This method also returns false if path is null, an invalid path,
        /// or a zero-length string. If the caller does not have sufficient
        /// permissions to read the specified file, no exception is thrown and
        /// the method returns false regardless of the existence of path.
        /// </summary>
        bool Exists(string filePath);
    }

    /// <summary>Extension methods for IReadOnlyFolder.</summary>
    public static class IReadOnlyFolderExtensions
    {
    }
}
