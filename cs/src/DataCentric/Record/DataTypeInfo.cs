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
using System.Linq;
using System.Net.Http.Headers;

namespace DataCentric
{
    /// <summary>
    /// Information about a data type obtained through reflection.
    ///
    /// This class can be used to obtain type information for classes
    /// derived from Data class, including through Key or Record classes.
    /// Using it with any other type will result in an error.
    /// </summary>
    public class DataTypeInfo
    {
        [ThreadStatic] private static Dictionary<Type, DataTypeInfo> dict_; // Do not initialize ThreadStatic here, initializer will run in first thread only

        private readonly Type type_;
        private readonly Type rootType_;

        //--- PROPERTIES

        /// <summary>
        /// Name of the database table or collection where the record is stored.
        ///
        /// By convention, this method returns the name of the class one level
        /// below TypedRecord in the inheritance chain, without namespace.
        ///
        /// Error message if called for a type that is not derived from TypedRecord.
        /// </summary>
        public string GetCollectionName()
        {
            if (DataKind != DataKindEnum.Record)
                throw new Exception(
                    $"GetCollectionName() method is called for {type_.Name} " +
                    $"that is not derived from TypedRecord.");

            return rootType_.Name;
        }

        /// <summary>Kind of the data type (record, key, or element).</summary>
        public DataKindEnum DataKind { get; }

        /// <summary>
        /// Inheritance chain from derived to base, ending
        /// with and inclusive of the RootType. This property
        /// is used to generate the value of _t BSON attribute.
        ///
        /// Root data type is the type that inherits directly
        /// from Key(TKey, TRecord).
        /// </summary>
        public string[] InheritanceChain { get; }

        /// <summary>
        /// Array of property info for all of the type's elements
        /// in the order of declaration.
        ///
        /// The elements are listed in the order of declaration
        /// within the class, and in the order from base to derived
        /// between the classes.
        /// </summary>
        public PropertyInfo[] DataElements { get; }

        /// <summary>
        /// Dictionary of property info for the elements of the
        /// root data type indexed by element name, including key
        /// elements and optionally also non-key elements.
        ///
        /// Root data type is the type that inherits directly
        /// from Key(TKey, TRecord).
        /// </summary>
        public Dictionary<string, PropertyInfo> DataElementDict { get; }

        /// <summary>
        /// Get cached instance for the specified object, or create
        /// using  and add to thread static cache if does not exist.
        ///
        /// This object contains information about the data type
        /// including the list of its elements (public properties
        /// that have one of the supported data types).
        /// </summary>
        public static DataTypeInfo GetOrCreate(object value)
        {
            return GetOrCreate(value.GetType());
        }

        /// <summary>
        /// Get cached instance for the specified type, or create
        /// using  and add to thread static cache if does not exist.
        ///
        /// This object contains information about the data type
        /// including the list of its elements (public properties
        /// that have one of the supported data types).
        ///
        /// This overload accepts type as generic parameter.
        /// </summary>
        public static DataTypeInfo GetOrCreate<TData>()
            where TData : Data
        {
            // Delegate to the overload taking Type
            return GetOrCreate(typeof(TData));
        }

        /// <summary>
        /// Get cached instance for the specified type, or create
        /// using  and add to thread static cache if does not exist.
        ///
        /// This object contains information about the data type
        /// including the list of its elements (public properties
        /// that have one of the supported data types).
        ///
        /// This overload accepts the value of Type as parameter.
        /// </summary>
        public static DataTypeInfo GetOrCreate(Type type)
        {
            // Check if thread static dictionary is already initialized
            if (dict_ == null)
            {
                // If not, initialize
                dict_ = new Dictionary<Type, DataTypeInfo>();
            }

            // Check if a cached instance exists in dictionary
            if (dict_.TryGetValue(type, out DataTypeInfo result))
            {
                // Return if found
                return result;
            }
            else
            {
                // Otherwise create and add to dictionary
                result = new DataTypeInfo(type);
                dict_.Add(type, result);
                return result;
            }
        }

        /// <summary>
        /// Create from type.
        ///
        /// This constructor is private because it is only called
        /// from the GetOrCreate(...) method. Users should rely
        /// on GetOrCreate(...) method only which uses thread static
        /// cached value if any, and creates the instance only if
        /// it is not yet cached for the thread.
        /// </summary>
        private DataTypeInfo(Type type)
        {
            type_ = type;

            // Populate the inheritance chain from parent to base,
            // stop when one of the base classes is reached or
            // there is no base class
            List<Type> inheritanceChain = new List<Type>();
            Type currentType = type;
            while (currentType.BaseType != null)
            {
                // Add type to the inheritance chain
                inheritanceChain.Add(currentType);

                Type baseType = currentType.BaseType;
                if (baseType == typeof(Data))
                {
                    if (rootType_ == null)
                    {
                        DataKind = DataKindEnum.Element;
                        rootType_ = currentType;
                    }
                }
                else if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(TypedKey<,>))
                {
                    if (rootType_ == null)
                    {
                        DataKind = DataKindEnum.Key;
                        rootType_ = currentType;

                        if (inheritanceChain.Count > 1)
                            throw new Exception(
                                $"Key type {type.Name} must be derived directly from TypedKey<TKey, TRecord> and sealed " +
                                $"because key classes cannot have an inheritance hierarchy, only data classes can.");
                    }
                }
                else if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(TypedRecord<,>))
                {
                    if (rootType_ == null)
                    {
                        DataKind = DataKindEnum.Record;
                        rootType_ = currentType;
                    }
                }

                currentType = currentType.BaseType;
            }

            // Add the root class to the inheritance chain
            if (currentType != null)
            {
                inheritanceChain.Add(currentType);
            }

            // Error message if the type is not derived from one of the permitted base classes
            if (DataKind == DataKindEnum.Empty)
                throw new Exception(
                    $"Data type {type.Name} is not derived from Data, TypedKey<TKey, TRecord>, or TypedRecord<TKey, TRecord>.");

            // Add elements in the order from from base to derived
            var dataElementList = new List<PropertyInfo>();
            for (int i = inheritanceChain.Count - 1; i >= 0; --i)
            {
                Type inheritanceChainEntry = inheritanceChain[i];

                // Find all properties of this type that are public, not static,
                // declared in this type only, and have both getter and setter.
                //
                // The query also expressly excludes Context and Key properties which
                // are not part of data and should not be serialized.
                var propInfoArray = inheritanceChainEntry.GetProperties(
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(p => (p.CanRead && p.CanWrite))
                    .Where(p => p.Name != "Context")
                    .Where(p => p.Name != "Key");

                foreach (var propInfo in propInfoArray)
                {
                    // This line detects if the property is an override and skips it so we avoid
                    // duplicate definitions for the original declaration and the override
                    var propGetter = propInfo.GetMethod;
                    var propGetterBaseDefinition = propGetter.GetBaseDefinition();
                    if (!propGetter.Equals(propGetterBaseDefinition))
                    {
                        // This is an override, skip at this level in the inheritance
                        // chain. The property will be added when its base definition
                        // is reached.
                        continue;
                    }

                    // DataElements has properties from all classes in the inheritance
                    // chain in the order of declaration, from base to derived
                    dataElementList.Add(propInfo);
                }
            }

            // Populate inheritance chain property with
            // names of types in the inheritance chain
            InheritanceChain = inheritanceChain.Select(p => p.Name).ToArray();

            // Populate data element list
            DataElements = dataElementList.ToArray();

            // Populate data element dictionary
            DataElementDict = new Dictionary<string, PropertyInfo>();
            foreach (var propertyInfo in dataElementList)
            {
                DataElementDict.Add(propertyInfo.Name, propertyInfo);
            }
        }
    }
}
