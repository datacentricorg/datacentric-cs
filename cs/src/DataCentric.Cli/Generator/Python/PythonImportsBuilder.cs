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
using Humanizer;

namespace DataCentric.Cli
{
    public static class PythonImportsBuilder
    {
        public static void WriteImports(TypeDecl decl, Dictionary<string, string> declPathDict, CodeWriter writer)
        {
            // If type is abstract - ABC import is needed
            if (decl.Kind == TypeKind.Abstract)
                writer.AppendLine("from abc import ABC");

            // Check if ObjectId is used
            bool hasObjectId = decl.Elements.Any(e => e.Value != null &&
                                                      (e.Value.Type == AtomicType.TemporalId ||
                                                       e.Value.Type == AtomicType.NullableTemporalId));
            if (hasObjectId)
                writer.AppendLine("from bson import ObjectId");

            bool hasList = decl.Elements.Any(e=>e.Vector == YesNo.Y);
            bool hasOptional = decl.Elements.Any(e => e.Value != null &&
                                                      (e.Value.Type == AtomicType.NullableBool ||
                                                       e.Value.Type == AtomicType.NullableDate ||
                                                       e.Value.Type == AtomicType.NullableDateTime ||
                                                       e.Value.Type == AtomicType.NullableDecimal ||
                                                       e.Value.Type == AtomicType.NullableDouble ||
                                                       e.Value.Type == AtomicType.NullableInt ||
                                                       e.Value.Type == AtomicType.NullableLong ||
                                                       e.Value.Type == AtomicType.NullableMinute ||
                                                       e.Value.Type == AtomicType.NullableTemporalId ||
                                                       e.Value.Type == AtomicType.NullableTime));

            // Check imports from typing
            if (hasList && hasOptional)
                writer.AppendLine("from typing import List, Optional");
            else if (hasList)
                writer.AppendLine("from typing import List");
            else if (hasOptional)
                writer.AppendLine("from typing import Optional");

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

            var samePackageData = decl.Elements
                                      .Where(e => e.Data != null &&
                                                  e.Data.Module.ModuleName == decl.Module.ModuleName)
                                      .GroupBy(e => e.Data.Name)
                                      .Select(g => g.First().Data)
                                      .ToList();

            foreach (var dataElement in samePackageData)
            {
                string key = dataElement.Module.ModuleName + "." + dataElement.Name;
                string elementModule = declPathDict[key];
                writer.AppendLine($"from {elementModule} import {dataElement.Name}");
            }

            var samePackageKeys = decl.Elements
                                      .Where(e => e.Key != null &&
                                                  e.Key.Module.ModuleName == decl.Module.ModuleName)
                                      .GroupBy(e => e.Key.Name)
                                      .Select(g => g.First().Key)
                                      .ToList();

            foreach (var element in samePackageKeys)
            {
                string key = element.Module.ModuleName + "." + element.Name;
                string elementModule = declPathDict[key];
                writer.AppendLine($"from {elementModule} import {element.Name}Key");
            }

            var samePackageEnums = decl.Elements
                                      .Where(e => e.Enum != null &&
                                                  e.Enum.Module.ModuleName == decl.Module.ModuleName)
                                      .GroupBy(e => e.Enum.Name)
                                      .Select(g => g.First().Enum)
                                      .ToList();

            foreach (var element in samePackageEnums)
            {
                string key = element.Module.ModuleName + "." + element.Name;
                string elementModule = declPathDict[key];
                writer.AppendLine($"from {elementModule} import {element.Name}");
            }

            // Import date-time classes
            if (insideDc)
            {
                var atomicElements = decl.Elements
                                         .Where(e => e.Value != null)
                                         .GroupBy(e => e.Value.Type)
                                         .Select(g => g.First().Value.Type)
                                         .ToArray();

                if (atomicElements.Contains(AtomicType.DateTime) || atomicElements.Contains(AtomicType.NullableDateTime))
                    writer.AppendLine("from datacentric.date_time.local_date_time import LocalDateTime");
                if (atomicElements.Contains(AtomicType.Date) || atomicElements.Contains(AtomicType.NullableDate))
                    writer.AppendLine("from datacentric.date_time.local_date import LocalDate");
                if (atomicElements.Contains(AtomicType.Time) || atomicElements.Contains(AtomicType.NullableTime))
                    writer.AppendLine("from datacentric.date_time.local_time import LocalTime");
                if (atomicElements.Contains(AtomicType.Minute) || atomicElements.Contains(AtomicType.NullableMinute))
                    writer.AppendLine("from datacentric.date_time.local_minute import LocalMinute");
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