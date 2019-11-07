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
using System.Runtime.CompilerServices;

namespace DataCentric
{
    /// <summary>
    /// Context for use in handlers.
    /// </summary>
    public class Context : IContext
    {
        private IFolder outputFolder_;
        private ILog log_;
        private IProgress progress_;
        private IDataSource dataSource_;
        private TemporalId? dataSet_;

        /// <summary>
        /// Provides a unified API for an output folder located in a
        /// conventional filesystem or an alternative backing store
        /// such as S3.
        /// </summary>
        public IFolder OutputFolder
        {
            get
            {
                if (outputFolder_ == null) throw new Exception($"OutputFolder property is not set in {GetType().Name}.");
                return outputFolder_;
            }
            set
            {
                outputFolder_ = value;
                outputFolder_.Init(this);
            }
        }

        /// <summary>Logging interface.</summary>
        public ILog Log
        {
            get
            {
                if (log_ == null) throw new Exception($"Log property is not set in {GetType().Name}.");
                return log_;
            }
            set
            {
                log_ = value;
                log_.Init(this);
            }
        }

        /// <summary>Progress interface.</summary>
        public IProgress Progress
        {
            get
            {
                if (progress_ == null) throw new Exception($"Progress property is not set in {GetType().Name}.");
                return progress_;
            }
            set
            {
                progress_ = value;
                progress_.Init(this);
            }
        }

        /// <summary>Default data source of the context.</summary>
        public IDataSource DataSource
        {
            get
            {
                if (dataSource_ == null) throw new Exception($"DataSource property is not set in {GetType().Name}.");
                return dataSource_;
            }
            set
            {
                dataSource_ = value;
                dataSource_.Init(this);
            }
        }

        /// <summary>Default dataset of the context.</summary>
        public TemporalId DataSet
        {
            get
            {
                if (dataSet_ == null) throw new Exception($"DataSet property is not set in {GetType().Name}.");
                return dataSet_.Value;
            }
            set
            {
                dataSet_ = value;
            }
        }

        //--- METHODS

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
            // Call Dispose() for each initialized property of the context
            // in the reverse order of initialization
            if (outputFolder_ != null) outputFolder_.Dispose();
            if (log_ != null) log_.Dispose();
            if (progress_ != null) progress_.Dispose();
            if (dataSource_ != null) dataSource_.Dispose();

            // Uncomment except in root class of the hierarchy
            // base.Dispose();
        }

        /// <summary>Flush data to permanent storage.</summary>
        public virtual void Flush()
        {
            // Uncomment except in root class of the hierarchy
            // base.Flush();

            // Call Flush() for each initialized property of the context
            if (outputFolder_ != null) outputFolder_.Flush();
            if (log_ != null) log_.Flush();
            if (progress_ != null) progress_.Flush();
            if (dataSource_ != null) dataSource_.Flush();
        }

        /// <summary>
        /// Invoke this method to keep test data after the
        /// test method exits.
        ///
        /// When running under xUnit, the data in test database is not
        /// erased on test method exit if KeepTestData() was invoked.
        ///
        /// When running under DataCentric, the test dataset will not
        /// be deleted on test method exit if KeepTestData() was invoked.
        ///
        /// Note that test data is always erased when test method enters,
        /// irrespective of any KeepTestData() calls and irrespective of
        /// whether or not KeepTestData() has been called.
        /// </summary>
        public virtual void KeepTestData()
        {
            throw new Exception("Method KeepTestData() invoked on a context that is not a unit test context.");
        }
    }
}
