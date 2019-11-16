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

namespace DataCentric
{
    /// <summary>
    /// Attribute for identifying methods that are handlers.
    ///
    /// Handlers are methods of a Record that can be invoked
    /// through the user interface or CLI.
    ///
    /// A handler method must return void and either take no
    /// parameters, or take parameters that are a combination
    /// of:
    ///
    /// * Atomic types
    /// * Classes derived from Data
    ///
    /// While passing Record types as handler parameters is not
    /// prohibited, best practice is to pass such parameters by
    /// specifying their key rather than their data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HandlerMethodAttribute : Attribute
    {
    }
}