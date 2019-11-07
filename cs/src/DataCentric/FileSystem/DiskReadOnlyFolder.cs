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
    /// Implements read-only IReadOnlyFolder interface for a conventional filesystem.
    ///
    /// To provide read-write interface, use DiskFolder class instead.
    /// </summary>
    public class DiskReadOnlyFolder : IReadOnlyFolder
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
        public IContext Context { get; private set; }

        /// <summary>Output folder path.</summary>
        public string FolderPath { get; set; }

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
        public virtual void Init(IContext context)
        {
            // Uncomment except in root class of the hierarchy
            // base.Init(context);

            // Check that argument is not null and assign to the Context property
            if (context == null) throw new Exception($"Null context is passed to the Init(...) method for {GetType().Name}.");
            Context = context;
        }

        /// <summary>
        /// Releases resources and calls base.Dispose().
        ///
        /// This method will not be called by the garbage collector.
        /// It will only be executed if:
        ///
        /// * This class implements IDisposable; and
        /// * The class instance is created through the using clause
        ///
        /// IMPORTANT - Every override of this method must call base.Dispose()
        /// after executing its own code.
        /// </summary>
        public virtual void Dispose()
        {
            // Uncomment except in root class of the hierarchy
            // base.Dispose();
        }

        /// <summary>
        /// True if the caller has the required permissions and path contains
        /// the name of an existing file; otherwise, false.
        ///
        /// This method also returns false if path is null, an invalid path,
        /// or a zero-length string. If the caller does not have sufficient
        /// permissions to read the specified file, no exception is thrown and
        /// the method returns false regardless of the existence of path.
        /// </summary>
        public bool Exists(string filePath)
        {
            string fullFilePath = Path.Combine(FolderPath, filePath);
            bool result = File.Exists(fullFilePath); // TODO - check behavior when the path is a directory
            return result;
        }
    }
}
