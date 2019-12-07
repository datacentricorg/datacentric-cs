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
using System.Runtime.CompilerServices;

namespace DataCentric
{
    /// <summary>
    /// Context for use in test fixtures that do not require a data
    /// source. Attempting to access DataSource property using this
    /// context will cause an error.
    ///
    /// This class extends Context with approval test functionality.
    /// </summary>
    public class UnitTestContext : Context
    {
        /// <summary>
        /// Name of the unit test method obtained using [CallerMemberName]
        /// attribute of the unit test method signature.
        /// </summary>
        public string TestMethodName { get; set; }

        /// <summary>
        /// Test class name obtained by parsing the [CallerFilePath]
        /// attribute of the unit test method signature.
        /// </summary>
        public string TestClassName { get; set; }

        /// <summary>
        /// Test folder name obtained by parsing the [CallerFilePath]
        /// attribute of the unit test method signature.
        /// </summary>
        public string TestFolderPath { get; set; }

        /// <summary>
        /// First argument is test object (instance of the unit test fixture).
        /// The remaining two arguments are provided by the compiler.
        /// </summary>
        public UnitTestContext(
            object testObj,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
        {
            // Check that properties required by the unit test are set
            if (testObj == null) throw new Exception("Class instance passed to UnitTestContext is null.");
            if (callerMemberName == null) throw new Exception("Method name passed to UnitTestContext is null.");
            if (callerFilePath == null) throw new Exception("Source file path passed to UnitTestContext is null.");

            // Split file path into test folder path and source filename
            string testFolderPath = Path.GetDirectoryName(callerFilePath);
            string sourceFileName = Path.GetFileName(callerFilePath);

            // Test class path is the path to source file followed by
            // subfolder whose name is source file name without extension
            if (!sourceFileName.EndsWith(".cs")) throw new Exception($"Source filename '{sourceFileName}' does not end with '.cs'");
            string testClassName = sourceFileName.Substring(0, sourceFileName.Length - 3);

            // Set properties of the unit test class for use by derived classes
            TestMethodName = callerMemberName;
            TestClassName = testClassName;
            TestFolderPath = testFolderPath;

            // Use log file name format className.MethodName.approved.txt from ApprovalTests.NET.
            string logFileName = String.Join(".", testClassName, callerMemberName, "approved.txt");

            // All properties must be set before initialization is performed
            // Do not call Init(...) here because they will be initialized by
            // inside the Context property setter
            OutputFolder = new DiskFolder { FolderPath = testFolderPath };
            Log = new FileLog { LogFilePath = logFileName };
            Progress = new NullProgress();

            // Increase log verbosity to Verify from its
            // default level set in base class Context.
            //
            // DO NOT move this to FileLog initialization
            // as it will get reset when Log.Init(...)
            // is called by Context.Log setter.
            Log.Verbosity = LogVerbosity.Verify;
        }
    }
}
