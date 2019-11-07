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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace DataCentric
{
    /// <summary>Assembly cache with deferred assemblies loading.</summary>
    public class AssemblyCache : IEnumerable<Assembly>
    {
        private class AssemblyIdentity
        {
            private Assembly assembly_;
            private string path_;

            public AssemblyIdentity(Assembly assembly)
            {
                assembly_ = assembly ?? throw new ArgumentException();
            }

            public AssemblyIdentity(string path)
            {
                if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException();
                path_ = path;
            }

            public Assembly Resolve(AssemblyLoadContext context)
            {
                if ((assembly_ == null) && (path_ != null))
                {
                    try
                    {
                        assembly_ = context.LoadFromAssemblyPath(path_);
                    }
                    catch (BadImageFormatException)
                    {
                        // Has no metainfo. i.e. not a .NET assembly and should be ignored
                        path_ = null;
                    }
                    catch (FileLoadException)
                    {
                        // Assembly recognized but not loaded, probably it's already loaded from other place
                        path_ = null;
                    }
                }
                return assembly_;
            }
        }

        private readonly AssemblyLoadContext context_;
        private readonly Queue<AssemblyIdentity> identityQueue_ = new Queue<AssemblyIdentity>();
        private readonly List<Assembly> assemblies_ = new List<Assembly>();
        private readonly HashSet<Assembly> duplicates_ = new HashSet<Assembly>();

        public AssemblyCache() : this(AssemblyLoadContext.Default) { }

        public AssemblyCache(AssemblyLoadContext context)
        {
            context_ = context ?? throw new ArgumentNullException(nameof(context));
            context_.Resolving += ResolveAssembly;
        }

        private Assembly ResolveAssembly(AssemblyLoadContext context, AssemblyName name)
        {
            foreach (AssemblyIdentity identity in identityQueue_)
            {
                Assembly assembly = identity.Resolve(context_);
                if ((assembly != null) && AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), name))
                {
                    return assembly;
                }
            }
            return null;
        }

        /// <summary>Registers known assembly in cache.</summary>
        public void AddAssembly(Assembly assembly)
        {
            identityQueue_.Enqueue(
                new AssemblyIdentity(assembly ?? throw new ArgumentNullException(nameof(assembly))));
        }

        /// <summary>Registers several known assemblies in cache.</summary>
        public void AddAssemblies(IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies ?? throw new ArgumentNullException(nameof(assemblies)))
            {
                identityQueue_.Enqueue(new AssemblyIdentity(assembly));
            }
        }

        /// <summary>Registers possible assembly file for deferred loading in cache.</summary>
        public void AddFile(string path)
        {
            identityQueue_.Enqueue(
                new AssemblyIdentity(path ?? throw new ArgumentNullException(nameof(path))));
        }

        /// <summary>Registers several possible assembly files for deferred loading in cache.</summary>
        public void AddFiles(IEnumerable<string> paths)
        {
            foreach (string path in paths ?? throw new ArgumentNullException(nameof(paths)))
            {
                identityQueue_.Enqueue(new AssemblyIdentity(path));
            }
        }

        /// <summary>Enumerate through all registered assemblies and load new ones when needed.</summary>
        public IEnumerator<Assembly> GetEnumerator()
        {
            foreach (Assembly assembly in assemblies_)
            {
                yield return assembly;
            }

            while (identityQueue_.Count > 0)
            {
                AssemblyIdentity identity = identityQueue_.Dequeue();
                Assembly assembly = identity.Resolve(context_);

                if ((assembly != null) && duplicates_.Add(assembly))
                {
                    assemblies_.Add(assembly);
                    yield return assembly;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsEmpty => (assemblies_.Count | identityQueue_.Count) == 0;
    }
}
