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
using System.Diagnostics;
using System.Text;

namespace DataCentric
{
    /// <summary>
    /// Provides a unified API for writing log output to:
    ///
    /// * Console
    /// * String
    /// * File
    /// * Database
    /// * Logging frameworks such as log4net and other logging frameworks
    /// * Cloud logging services such as AWS CloudWatch
    /// </summary>
    public interface ILog : IDisposable
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

        /// <summary>
        /// Minimal verbosity for which log entry will be displayed.
        /// </summary>
        LogVerbosityEnum? Verbosity { get; set; }

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

        /// <summary>Flush data to permanent storage.</summary>
        void Flush();

        /// <summary>
        /// Publish the specified entry to the log if log verbosity
        /// is the same or high as entry verbosity.
        ///
        /// When log entry data is passed to this method, only the following
        /// elements are required:
        ///
        /// * Verbosity
        /// * Title (should not have line breaks; if found will be replaced by spaces)
        /// * Description (line breaks and formatting will be preserved)
        ///
        /// The remaining fields of LogEntry will be populated if the log
        /// entry is published to a data source. They are not necessary if the
        /// log entry is published to a text log.
        ///
        /// In a text log, the first line of each log entry is Verbosity
        /// followed by semicolon separator and then Title of the log entry.
        /// Remaining lines are Description of the log entry recorded with
        /// 4 space indent but otherwise preserving its formatting.
        ///
        /// Example:
        ///
        /// Info: Sample Title
        ///     Sample Description Line 1
        ///     Sample Description Line 2
        /// </summary>
        void Publish(LogEntry logEntry);
    }

    /// <summary>Extension methods for ILog.</summary>
    public static class ILogExtensions
    {
        /// <summary>
        /// Publish a new entry to the log if log verbosity
        /// is the same or high as entry verbosity.
        ///
        /// In a text log, each log entry is Verbosity followed
        /// by semicolon separator and then Title.
        ///
        /// Example:
        ///
        /// Info: Sample Title
        /// </summary>
        public static void Publish(this ILog obj, LogVerbosityEnum verbosity, string title)
        {
            // Invoke the overload with message title and body
            // and pass null for the body variable
            obj.Publish(verbosity, title, null);
        }

        /// <summary>
        /// Publish a new entry to the log if log verbosity
        /// is the same or high as entry verbosity.
        ///
        /// In a text log, the first line of each log entry is Verbosity
        /// followed by semicolon separator and then Title of the log entry.
        /// Remaining lines are Description of the log entry recorded with
        /// 4 space indent but otherwise preserving its formatting.
        ///
        /// Example:
        ///
        /// Info: Sample Title
        ///     Sample Description Line 1
        ///     Sample Description Line 2
        /// </summary>
        public static void Publish(this ILog obj, LogVerbosityEnum verbosity, string title, string description)
        {
            // Populate only those fields of of the log entry that are passed to this method.
            // The remaining fields will be populated if the log entry is published to a data
            // source. They are not necessary if the log entry is published to a text log.
            var logEntry = new LogEntry {Verbosity = verbosity, Title = title, Description = description};

            // Publish the log entry to the log
            obj.Publish(logEntry);
        }

        /// <summary>
        /// Publish an error message to the log for any log verbosity.
        ///
        /// This method does not throw an exception; it is invoked
        /// to indicate an error when exception is not necessary,
        /// and it may also be invoked when the exception is caught.
        ///
        /// In a text log, each log entry is Verbosity followed
        /// by semicolon separator and then Title.
        ///
        /// Example:
        ///
        /// Error: Sample Title
        /// </summary>
        public static void Error(this ILog obj, string title)
        {
            // Published at any level of verbosity
            obj.Publish(LogVerbosityEnum.Error, title);
        }

        /// <summary>
        /// Publish an error message to the log for any log verbosity.
        ///
        /// This method does not throw an exception; it is invoked
        /// to indicate an error when exception is not necessary,
        /// and it may also be invoked when the exception is caught.
        ///
        /// In a text log, the first line of each log entry is Verbosity
        /// followed by semicolon separator and then Title of the log entry.
        /// Remaining lines are Description of the log entry recorded with
        /// 4 space indent but otherwise preserving its formatting.
        ///
        /// Example:
        ///
        /// Error: Sample Title
        ///     Sample Description Line 1
        ///     Sample Description Line 2
        /// </summary>
        public static void Error(this ILog obj, string title, string description)
        {
            // Published at any level of verbosity
            obj.Publish(LogVerbosityEnum.Error, title, description);
        }

        /// <summary>
        /// Publish a warning message to the log if log verbosity
        /// is at least Warning.
        ///
        /// Warning messages should be used sparingly to avoid
        /// flooding log output with insignificant warnings.
        /// A warning message should never be generated inside
        /// a loop.
        ///
        /// In a text log, each log entry is Verbosity followed
        /// by semicolon separator and then Title.
        ///
        /// Example:
        ///
        /// Warning: Sample Title
        /// </summary>
        public static void Warning(this ILog obj, string title)
        {
            // Requires at least Warning verbosity
            obj.Publish(LogVerbosityEnum.Warning, title);
        }

        /// <summary>
        /// Publish a warning message to the log if log verbosity
        /// is at least Warning.
        ///
        /// Warning messages should be used sparingly to avoid
        /// flooding log output with insignificant warnings.
        /// A warning message should never be generated inside
        /// a loop.
        ///
        /// In a text log, the first line of each log entry is Verbosity
        /// followed by semicolon separator and then Title of the log entry.
        /// Remaining lines are Description of the log entry recorded with
        /// 4 space indent but otherwise preserving its formatting.
        ///
        /// Example:
        ///
        /// Warning: Sample Title
        ///     Sample Description Line 1
        ///     Sample Description Line 2
        /// </summary>
        public static void Warning(this ILog obj, string title, string description)
        {
            // Requires at least Warning verbosity
            obj.Publish(LogVerbosityEnum.Warning, title, description);
        }

        /// <summary>
        /// Publish an info message to the log if log verbosity
        /// is at least Info.
        ///
        /// Info messages should be used sparingly to avoid
        /// flooding log output with superfluous data. An info
        /// message should never be generated inside a loop.
        ///
        /// In a text log, each log entry is Verbosity followed
        /// by semicolon separator and then Title.
        ///
        /// Example:
        ///
        /// Info: Sample Title
        /// </summary>
        public static void Info(this ILog obj, string title)
        {
            // Requires at least Info verbosity
            obj.Publish(LogVerbosityEnum.Info, title);
        }

        /// <summary>
        /// Publish an info message to the log if log verbosity
        /// is at least Info.
        ///
        /// Info messages should be used sparingly to avoid
        /// flooding log output with superfluous data. An info
        /// message should never be generated inside a loop.
        ///
        /// In a text log, the first line of each log entry is Verbosity
        /// followed by semicolon separator and then Title of the log entry.
        /// Remaining lines are Description of the log entry recorded with
        /// 4 space indent but otherwise preserving its formatting.
        ///
        /// Example:
        ///
        /// Info: Sample Title
        ///     Sample Description Line 1
        ///     Sample Description Line 2
        /// </summary>
        public static void Info(this ILog obj, string title, string description)
        {
            // Requires at least Info verbosity
            obj.Publish(LogVerbosityEnum.Info, title, description);
        }

        /// <summary>
        /// Publish a verification message to the log if log verbosity
        /// is at least Verify.
        ///
        /// In a text log, each log entry is Verbosity followed
        /// by semicolon separator and then Title.
        ///
        /// Example:
        ///
        /// Verify: Sample Title
        /// </summary>
        public static void Verify(this ILog obj, string title)
        {
            // Requires at least Verify verbosity
            obj.Publish(LogVerbosityEnum.Verify, title);
        }

        /// <summary>
        /// Publish a verification message to the log if log verbosity
        /// is at least Verify.
        ///
        /// In a text log, the first line of each log entry is Verbosity
        /// followed by semicolon separator and then Title of the log entry.
        /// Remaining lines are Description of the log entry recorded with
        /// 4 space indent but otherwise preserving its formatting.
        ///
        /// Example:
        ///
        /// Verify: Sample Title
        ///     Sample Description Line 1
        ///     Sample Description Line 2
        /// </summary>
        public static void Verify(this ILog obj, string title, string description)
        {
            // Requires at least Verify verbosity
            obj.Publish(LogVerbosityEnum.Verify, title, description);
        }

        /// <summary>
        /// If condition is false, record an error message for any
        /// verbosity. If condition is true, record a verification
        /// message to the log if log verbosity is at least Verify.
        ///
        /// In a text log, each log entry is Verbosity followed
        /// by semicolon separator and then Title.
        ///
        /// Example:
        ///
        /// Verify: Sample Title
        /// </summary>
        public static void Assert(this ILog obj, bool condition, string title)
        {
            // Requires at least Verify verbosity if condition is true
            if (!condition) obj.Error(title);
            else obj.Verify(title);
        }

        /// <summary>
        /// If condition is false, record an error message for any
        /// verbosity. If condition is true, record a verification
        /// message to the log if log verbosity is at least Verify.
        ///
        /// In a text log, the first line of each log entry is Verbosity
        /// followed by semicolon separator and then Title of the log entry.
        /// Remaining lines are Description of the log entry recorded with
        /// 4 space indent but otherwise preserving its formatting.
        ///
        /// Example:
        ///
        /// Verify: Sample Title
        ///     Sample Description Line 1
        ///     Sample Description Line 2
        /// </summary>
        public static void Assert(this ILog obj, bool condition, string title, string description)
        {
            // Requires at least Verify verbosity if condition is true
            if (!condition) obj.Error(title, description);
            else obj.Verify(title, description);
        }
    }
}
