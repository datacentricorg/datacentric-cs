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
using Humanizer;

namespace DataCentric.Cli
{
    public static class PythonImportsBuilder
    {
        public static void WriteImports(TypeDecl decl, Dictionary<string, string> declPathDict, CodeWriter writer)
        {
            bool insideDc = decl.Module.ModuleName == "DataCentric";

            bool parentClassInDifferentModule =
                decl.Inherit != null && decl.Inherit.Module.ModuleName != decl.Module.ModuleName;
            string parentClassPackage = parentClassInDifferentModule
                                            ? GetPythonPackage(decl.Inherit.Module.ModuleName) : null;
            string parentClassNamespace = parentClassInDifferentModule
                                              ? GetPythonNamespace(decl.Inherit.Module.ModuleName) : null;

            // Import parent class package as its namespace, or if inside datacentric,
            // import individual class instead
            if (decl.Inherit != null)
            {
                if (insideDc)
                {
                    // Import parent package namespace unless it is the same as
                    // the namespace for the class itself
                    if (decl.Module.ModuleName == decl.Inherit.Module.ModuleName)
                    {
                        // Parent class name and filename based on converting
                        // class name to snake case
                        string parentClassName = decl.Inherit.Name;
                        string key = decl.Inherit.Module.ModuleName + "." + decl.Inherit.Name;
                        string parentPythonFileName = declPathDict[key];

                        // Import individual parent class if package namespace is
                        // the same as parent class namespace. Use ? as the folder
                        // is unknown, this will be corrected after the generation
                        writer.AppendLine($"from {parentPythonFileName} import {parentClassName}");
                    }
                    else
                        throw new Exception("When generating code for the datacentric package, " +
                                            "parent packages should not be managed via a declaration.");
                }
                else
                {
                    // Import parent package namespace unless it is the same as
                    // the namespace for the class itself
                    if (decl.Module.ModuleName == decl.Inherit.Module.ModuleName)
                    {
                        // Parent class name and filename based on converting
                        // class name to snake case
                        string parentClassName = decl.Inherit.Name;
                        string key = decl.Inherit.Module.ModuleName + "." + decl.Inherit.Name;
                        string parentPythonFileName = declPathDict[key];

                        // Import individual parent class if package namespace is
                        // the same as parent class namespace. Use ? as the folder
                        // is unknown, this will be corrected after the generation
                        writer.AppendLine($"from {parentPythonFileName} import {parentClassName}");
                    }
                    else
                    {
                        // Otherwise import the entire package of the parent class
                        writer.AppendLine($"import {parentClassPackage} as {parentClassNamespace}");
                    }
                }
            }
            // Import datacentric package as dc, or if inside datacentric,
            // import individual classes instead
            else if (decl.IsRecord)
            {
                if (insideDc)
                {
                    writer.AppendLine("from datacentric.storage.typed_key import TypedKey");
                    writer.AppendLine("from datacentric.storage.typed_record import TypedRecord");
                }
                else
                {
                    writer.AppendLine("import datacentric as dc");
                }
            }
            else
            {
                if (insideDc)
                {
                    writer.AppendLine("from datacentric.storage.data import Data");
                }
                else
                {
                    writer.AppendLine("import datacentric as dc");
                }
            }
        }

        public static string GetPythonPackage(string moduleName)
        {
            switch (moduleName)
            {
                case "DataCentric": return "datacentric";
                default:            return "unknown_module"; // TODO - resolve all and raise an error if not found
            }
        }

        public static string GetPythonNamespace(string moduleName)
        {
            switch (moduleName)
            {
                case "DataCentric": return "dc";
                default:            return "unknown_module"; // TODO - resolve all and raise an error if not found
            }
        }
    }
}