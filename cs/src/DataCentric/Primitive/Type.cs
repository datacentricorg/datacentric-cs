using System;
using System.Collections.Generic;

namespace DataCentric
{
    /// <summary>Extension methods for Type.</summary>
    public static class TypeExtensions
    {
        /// <summary>Get generic argument by index.</summary>
        public static Type GetGenericArgument(this Type type, int index)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GetGenericArguments()[index];
        }

        /// <summary>Find parent interfaces.</summary>
        public static IEnumerable<Type> FindInterfaces(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (interfaceType.IsInterface)
            {
                if (interfaceType.IsGenericTypeDefinition)
                {
                    return type.FindInterfaces(
                        (i, c) => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType, null);
                }
                return type.FindInterfaces((i, c) => i == interfaceType, null);
            }
            return Array.Empty<Type>();
        }
    }
}
