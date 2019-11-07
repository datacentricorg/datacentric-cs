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
using Xunit;
using DataCentric;
using NodaTime;

namespace DataCentric.Test
{
    /// <summary>Sample base class implementing IDisposable.</summary>
    public class DisposableTestBaseSample : IDisposable
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
        public IContext Context { get; }

        /// <summary>Create from context.</summary>
        public DisposableTestBaseSample(IContext context)
        {
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
            Context.Log.Verify("Disposing resources of BaseClassSample");

            // Uncomment except in root class of the hierarchy
            // base.Dispose();
        }
    }

    /// <summary>Sample derived class implementing IDisposable.</summary>
    public class DisposableTestDerivedSample : DisposableTestBaseSample
    {
        /// <summary>Create from context.</summary>
        public DisposableTestDerivedSample(IContext context) : base(context)
        {
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
        public override void Dispose()
        {
            Context.Log.Verify("Disposing resources of DerivedClassSample");

            // Dispose base
            base.Dispose();
        }
    }

    /// <summary>Unit test for Disposable.</summary>
    public class DisposableTest
    {
        /// <summary>Test disposing resources via the ``using'' clause.</summary>
        [Fact]
        public void Smoke()
        {
            using (var context = new UnitTestContext(this))
            {
                // Base class by its own reference
                using (DisposableTestBaseSample obj = new DisposableTestBaseSample(context)) { }

                // Derived class by its own reference
                using (DisposableTestDerivedSample obj = new DisposableTestDerivedSample(context)) { }

                // Derived class by base reference
                using (DisposableTestBaseSample obj = new DisposableTestDerivedSample(context)) { }
            }
        }
    }
}
