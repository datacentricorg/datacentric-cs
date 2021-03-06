﻿/*
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

using System.Collections.Generic;
using System.Linq;

namespace DataCentric
{
    /// <summary>
    /// Provides import statements for generated classes.
    /// </summary>
    public static class PythonImportsBuilder
    {
        /// <summary>
        /// Add import statements for given declaration.
        /// </summary>
        public static void WriteImports(TypeDecl decl, List<IDecl> declarations, CodeWriter writer)
        {
            // Always import attr module
            writer.AppendLine("import attr");


            // Instant is generated as Union[dt.datetime, dc.Instant] thus dt import is required
            if (decl.Elements.Any(e => e.Value != null && (e.Value.Type == ValueParamType.Instant ||
                                                           e.Value.Type == ValueParamType.NullableInstant)))
                writer.AppendLine("import datetime as dt");

            // If type is abstract - ABC import is needed
            if (decl.Kind == TypeKind.Abstract)
                writer.AppendLine("from abc import ABC");

            // Check if ObjectId is used
            bool hasObjectId = decl.Elements.Any(e => e.Value != null &&
                                                      (e.Value.Type == ValueParamType.TemporalId ||
                                                       e.Value.Type == ValueParamType.NullableTemporalId));
            if (hasObjectId)
                writer.AppendLine("from bson import ObjectId");

            // Check imports from typing
            var typingImports = new List<string>();

            // Python 3.8
            // if (decl.Keys.Any() || decl.Kind == TypeKind.Final)
            //     typingImports.Add("final");

            if (decl.Elements.Any(e=>e.Vector == YesNo.Y))
                typingImports.Add("List");

            if (typingImports.Any())
            {
                var items = string.Join(", ", typingImports);
                writer.AppendLine($"from typing import {items}");
            }

            bool insideDc = PyExtensions.GetPackage(decl) == "datacentric";

            List<string> packagesToImport = new List<string>();
            List<string> individualImports = new List<string>();

            // Import parent class package as its namespace, or if inside the same package,
            // import individual class instead
            if (decl.Inherit != null)
            {
                if (PyExtensions.IsPackageEquals(decl, decl.Inherit))
                {
                    IDecl parentDecl = declarations.FindByKey(decl.Inherit);
                    individualImports.Add($"from {parentDecl.Category} import {decl.Inherit.Name}");
                }
                else
                {
                    packagesToImport.Add(PyExtensions.GetPackage(decl.Inherit));
                }
            }
            // Import datacentric package as dc, or if inside datacentric,
            // import individual classes instead
            else if (decl.IsRecord)
            {
                if (insideDc)
                {
                    individualImports.Add("from datacentric.storage.record import Record");
                }
                else
                {
                    packagesToImport.Add("datacentric");
                }
            }
            // First child class of Data
            else
            {
                if (insideDc)
                {
                    individualImports.Add("from datacentric.storage.data import Data");
                }
                else
                {
                    packagesToImport.Add("datacentric");
                }
            }

            foreach (var data in decl.Elements.Where(d => d.Data != null).Select(d => d.Data))
            {
                if (PyExtensions.IsPackageEquals(decl, data))
                {
                    IDecl dataDecl = declarations.FindByKey(data);
                    individualImports.Add($"from {dataDecl.Category} import {data.Name}");
                }
                else
                    packagesToImport.Add(PyExtensions.GetPackage(data));
            }

            foreach (var enumElement in decl.Elements.Where(d => d.Enum != null).Select(d => d.Enum))
            {
                if (PyExtensions.IsPackageEquals(decl, enumElement))
                {
                    IDecl enumDecl = declarations.FindByKey(enumElement);
                    individualImports.Add($"from {enumDecl.Category} import {enumElement.Name}");
                }
                else
                    packagesToImport.Add(PyExtensions.GetPackage(enumElement));
            }

            foreach (var package in packagesToImport.Distinct())
            {
                writer.AppendLine($"import {package} as {PyExtensions.GetAlias(package)}");
            }

            foreach (var import in individualImports.Distinct())
            {
                writer.AppendLine(import);
            }
        }
    }
}