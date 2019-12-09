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

namespace DataCentric
{
    /// <summary>
    /// Helper class for python generator.
    /// </summary>
    public static class PyExtensions
    {
        /// <summary>
        /// Check if provided entities belong to the same package.
        /// </summary>
        public static bool IsPackageEquals(IDecl decl, IDeclKey declKey)
        {
            return GetPackage(decl) == GetPackage(declKey);
        }

        /// <summary>
        /// Return top level package name for given declaration.
        /// </summary>
        public static string GetPackage(IDecl decl)
        {
            if (decl.Module.ModuleName.StartsWith("DataCentric"))
                return "datacentric";
            throw new Exception($"Unknown module: {decl.Module.ModuleName}");
        }

        /// <summary>
        /// Return top level package name for a given declaration key.
        /// </summary>
        public static string GetPackage(IDeclKey declKey)
        {
            if (declKey.Module.ModuleName.StartsWith("DataCentric"))
                return "datacentric";
            // NodaTime classes should be implemented in datacentric-py package.
            if (declKey.Module.ModuleName.StartsWith("NodaTime"))
                return "datacentric";
            throw new Exception($"Unknown module: {declKey.Module.ModuleName}");
        }

        /// <summary>
        /// Returns alias for declaration key.
        /// </summary>
        public static string GetAlias(IDeclKey declKey)
        {
            if (declKey.Module.ModuleName.StartsWith("DataCentric"))
                return "dc";
            // NodaTime classes should be implemented in python datacentric package.
            if (declKey.Module.ModuleName.StartsWith("NodaTime"))
                return "dc";
            throw new Exception($"Unknown module: {declKey.Module.ModuleName}");
        }

        /// <summary>
        /// Returns alias for a known package.
        /// </summary>
        public static string GetAlias(string package)
        {
            switch (package)
            {
                case "datacentric": return "dc";
                default:            throw new Exception($"Unknown module: {package}");
            }
        }

        /// <summary>
        /// Find single declaration by its module and name.
        /// </summary>
        public static IDecl FindByKey(this List<IDecl> declarations, IDeclKey key)
        {
            return declarations.Single(d => d.Module.ModuleName == key.Module.ModuleName && d.Name == key.Name);
        }
    }
}