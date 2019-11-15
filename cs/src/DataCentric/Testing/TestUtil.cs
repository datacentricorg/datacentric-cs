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
using MongoDB.Bson.Serialization.Attributes;

namespace DataCentric
{
    /// <summary>
    /// Helper class for unit testing.
    /// </summary>
    public static class TestUtil
    {
        /// <summary>
        /// Generates random seed using hashcode of class name and method name,
        /// where method name is passed as implicit parameters to this method.
        ///
        /// The purpose of this method is to provide the ability to set seed for
        /// randomly generated test data without using the same seed for different
        /// test classes and methods.
        /// </summary>
        public static int GetRandomSeed(
            object obj,
            [CallerMemberName] string methodName = null)
        {
            if (string.IsNullOrEmpty(methodName))
                throw new Exception("Empty method name is passed to GetRandomSeed method.");

            // Get seed as hash of ClassName.Method name string. Using hashcode of a well defined
            // string makes it possible to reproduce the same seed in a different programming
            // language for consistent test data across languages
            string fullClassName = string.Join(".", obj.GetType(), obj.GetType(), methodName);
            int result = fullClassName.GetHashCode();
            return result;
        }
    }
}
