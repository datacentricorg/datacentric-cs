using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataCentric
{
    /// <summary>Static helper class for working with the activator.</summary>
    public static class ActivatorUtil
    {
        /// <summary>Get a list of parameter values from a list of their names for a given method.</summary>
        public static object[] CreateParameterValues(MethodInfo method, IEnumerable<string> parameters)
        {
            ParameterInfo[] descriptions = method.GetParameters();
            object[] values = new object[descriptions.Length];
            Dictionary<string, string> map = null;

            using (IEnumerator<string> seq = parameters.GetEnumerator())
            {
                for (int i = 0; i < descriptions.Length; i++)
                {
                    ParameterInfo description = descriptions[i];
                    string value = null;

                    while (seq.MoveNext())
                    {
                        if (ParamKeyValueParser.TryParsePair(seq.Current, out string paramName, out string paramValue))
                        {
                            if (string.Equals(paramName, description.Name, StringComparison.Ordinal))
                            {
                                value = paramValue;
                                break;
                            }
                            else
                            {
                                if (map == null)
                                {
                                    map = new Dictionary<string, string>(StringComparer.Ordinal);
                                }
                                map.Add(paramName, paramValue);
                            }
                        }
                        else
                        {
                            if (map == null)
                            {
                                value = seq.Current;
                                break;
                            }
                            throw new ArgumentException("Invalid parameters sequence");
                        }
                    }

                    if ((value != null) || ((map != null) && map.TryGetValue(description.Name, out value)))
                    {
                        values[i] = Convert.ChangeType(value, description.ParameterType);
                    }
                    else
                    {
                        if (description.HasDefaultValue)
                        {
                            values[i] = description.DefaultValue;
                        }
                        else
                        {
                            throw new ArgumentException($"Parameter {description.Name} value missing");
                        }
                    }
                }
            }

            return values;
        }

        /// <summary>Enumerate types in an assembly.</summary>
        public static IEnumerable<Type> EnumerateTypes(Assembly assembly)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            foreach (Type type in types)
            {
                if (type != null)
                {
                    yield return type;
                }
            }
        }

        /// <summary>Get type by name from a list of assemblies.</summary>
        public static Type ResolveType(string typeName, IEnumerable<Assembly> assemblies)
        {
            if (typeName.IndexOf(',') > 0)
            {
                // Full type name with assembly name
                return Type.GetType(typeName);
            }

            if (typeName.IndexOf('.') > 0)
            {
                // Full type name but without assembly name
                foreach (Assembly assembly in assemblies)
                {
                    Type type = assembly.GetType(typeName);
                    if (type != null)
                    {
                        return type;
                    }
                }
            }
            else
            {
                // Type name only
                foreach (Assembly assembly in assemblies)
                {
                    foreach (Type type in EnumerateTypes(assembly))
                    {
                        if (string.Equals(type.Name, typeName, StringComparison.Ordinal))
                        {
                            return type;
                        }
                    }
                }
            }

            // No type found
            return null;
        }
    }
}
