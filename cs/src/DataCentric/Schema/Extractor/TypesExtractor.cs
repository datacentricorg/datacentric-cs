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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DataCentric.Schema.Extractor
{
    /// <summary>
    /// Class to extract specific types from assemblies.
    /// </summary>
    public static class TypesExtractor
    {
        /// <summary>
        /// List of basic aka root types of type system.
        /// </summary>
        public static readonly List<Type> BasicTypes = new List<Type>
        {
            typeof(Data),
            typeof(Key),
            typeof(Record),
            typeof(RootRecord<,>),
            typeof(TypedRecord<,>),
            typeof(TypedKey<,>),
            typeof(DeletedRecord),
            typeof(GenericRecord),
        };

        /// <summary>
        /// Extracts data types with specified filter from assembly.
        /// </summary>
        public static List<Type> GetTypes(Assembly assembly, IEnumerable<string> filters)
        {
            Regex nameFilter = CreateTypeNameFilter(filters);

            bool IsKeyType(Type t) => t.BaseType.IsGenericType &&
                                      (t.BaseType.GetGenericTypeDefinition() == typeof(TypedKey<,>));

            return ActivatorUtil.EnumerateTypes(assembly)
                               // Get all Data successors
                              .Where(t => t.IsSubclassOf(typeof(Data)))
                               // Filter using user input or skip if none
                              .Where(t => nameFilter == null || nameFilter.IsMatch(t.FullName))
                              // Skip key and basic classes
                              .Where(t => !BasicTypes.Contains(t) && !IsKeyType(t)).ToList();
        }

        /// <summary>
        /// Extracts enum types with specified filter from assembly.
        /// </summary>
        public static List<Type> GetEnums(Assembly assembly, IEnumerable<string> filters)
        {
            Regex nameFilter = CreateTypeNameFilter(filters);

            return ActivatorUtil.EnumerateTypes(assembly)
                               // Get all Data successors
                              .Where(t => t.IsSubclassOf(typeof(Enum)))
                               // Filter using user input or skip if none
                              .Where(t => nameFilter == null || nameFilter.IsMatch(t.FullName))
                              .ToList();
        }

        /// <summary>
        /// Extracts interface types with specified filter from assembly.
        /// </summary>
        public static List<Type> GetInterfaces(Assembly assembly, IEnumerable<string> filters, IEnumerable<Type> dataTypes)
        {
            Regex nameFilter = CreateTypeNameFilter(filters);

            var interfaces = ActivatorUtil.EnumerateTypes(assembly).Where(t => t.IsInterface)
                                        .Where(t => nameFilter == null || nameFilter.IsMatch(t.FullName))
                                        .ToList();

            HashSet<Type> dataInterfaces = new HashSet<Type>();
            foreach (Type type in dataTypes)
            {
                foreach (Type interfaceType in interfaces)
                {
                    // Interface should be assignable from data type
                    bool isAssignableFromData = interfaceType.IsAssignableFrom(type);
                    // Skip RecordFor<> interfaces
                    bool isAssignableFromRecord = interfaceType.IsAssignableFrom(typeof(TypedRecord<,>));
                    if (isAssignableFromData && !isAssignableFromRecord)
                    {
                        dataInterfaces.Add(interfaceType);
                    }
                }
            }

            return dataInterfaces.ToList();
        }

        /// <summary>
        /// Translates user provided filter for types into regex.
        /// </summary>
        public static Regex CreateTypeNameFilter(IEnumerable<string> types)
        {
            StringBuilder filterBuilder;

            using (IEnumerator<string> typeEnumerator = types.GetEnumerator())
            {
                if (!typeEnumerator.MoveNext())
                    return null;

                filterBuilder = new StringBuilder();
                filterBuilder.Append('^');
                filterBuilder.Append('(');
                StringBuilder nameBuilder = new StringBuilder();

                do
                {
                    string typeName = typeEnumerator.Current;

                    if ((typeName.Length == 1) && (typeName[0] == '*'))
                        return null;

                    nameBuilder.Clear();
                    bool partialName = true;

                    foreach (char nextChar in typeName)
                    {
                        if (nextChar == '.')
                        {
                            partialName = false;
                            nameBuilder.Append('\\');
                            nameBuilder.Append('.');
                        }
                        else if (nextChar == '?')
                        {
                            nameBuilder.Append('.');
                        }
                        else if (nextChar == '*')
                        {
                            nameBuilder.Append('.');
                            nameBuilder.Append('*');
                        }
                        else
                        {
                            nameBuilder.Append(Regex.Escape(nextChar.ToString()));
                        }
                    }

                    if (partialName)
                    {
                        filterBuilder.Append('.');
                        filterBuilder.Append('*');
                        filterBuilder.Append('\\');
                        filterBuilder.Append('.');
                    }
                    filterBuilder.Append(nameBuilder);

                    filterBuilder.Append('|');
                }
                while (typeEnumerator.MoveNext());
            }

            filterBuilder.Length--;
            filterBuilder.Append(')');
            filterBuilder.Append('$');

            return new Regex(filterBuilder.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}