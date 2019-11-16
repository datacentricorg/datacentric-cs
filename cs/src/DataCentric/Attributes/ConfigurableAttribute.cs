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
using System.Reflection;

namespace DataCentric
{
    /// <summary>
    /// For every class marked with [Configurable] attribute, Context.Configure()
    /// will invoke static Configure(context) method with self as argument for
    /// every class that is accessible by the executing assembly and marked
    /// with [Configurable] attribute.
    ///
    /// The method Configure(context) configures:
    ///
    /// * Reference data, and
    /// * In case of test mocks, test data
    ///
    /// The order in which Configure(context) method is invoked when
    /// multiple classes marked by [Configurable] attribute are present
    /// is undefined. The implementation of Configure(context) should
    /// not rely on any existing data, and should not invoke other
    /// Configure(context) method of other classes.
    ///
    /// The attribute [Configurable] is not inherited. To invoke
    /// Configure(context) method for multiple classes within the same
    /// inheritance chain, specify [Configurable] attribute for each
    /// class that provides Configure(context) method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ConfigurableAttribute : Attribute
    {
    }
}