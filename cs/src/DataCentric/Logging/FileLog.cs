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
using System.CodeDom.Compiler;
using System.IO;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>Writes log output to the specified text file as it arrives.</summary>
    public sealed class FileLog : TextLog
    {
        //--- PROPERTIES

        /// <summary>Log file path relative to output folder root.</summary>
        [BsonRequired]
        public string LogFilePath { get; set; }

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
        public override void Init(IContext context)
        {
            // Initialize base
            base.Init(context);

            // Assign text writer for the log file
            textWriter_ = context.OutputFolder.GetTextWriter(LogFilePath, FileWriteModeEnum.Replace);
        }
    }
}
