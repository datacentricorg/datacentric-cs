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
using System.Reflection;

namespace DataCentric
{
    /// <summary>Searches for assembly files in the same folder as the executing assembly.</summary>
    public static class ActivatorSettings
    {
        /// <summary>Searches for assembly files in the same folder as the executing assembly.</summary>
        public static IEnumerable<Assembly> Assemblies
        {
            get
            {
                var assemblyCache = new AssemblyCache();
                assemblyCache.AddAssembly(Assembly.GetExecutingAssembly());
                // FIXME
                // TODO: File extension for Linux?
                assemblyCache.AddFiles(Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"));

                return assemblyCache;
            }
        }
    }
}