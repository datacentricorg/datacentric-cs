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
            if (decl.Elements.Any(e => e.Value != null && (e.Value.Type == AtomicType.Instant ||
                                                           e.Value.Type == AtomicType.NullableInstant)))
                writer.AppendLine("import datetime as dt");

            // If type is abstract - ABC import is needed
            if (decl.Kind == TypeKind.Abstract)
                writer.AppendLine("from abc import ABC");

            // Check if ObjectId is used
            bool hasObjectId = decl.Elements.Any(e => e.Value != null &&
                                                      (e.Value.Type == AtomicType.TemporalId ||
                                                       e.Value.Type == AtomicType.NullableTemporalId));
            if (hasObjectId)
                writer.AppendLine("from bson import ObjectId");

            // Check imports from typing
            var typingImports = new List<string>();

            // Python 3.8
            // if (decl.Keys.Any() || decl.Kind == TypeKind.Final)
            //     typingImports.Add("final");

            if (decl.Elements.Any(e=>e.Vector == YesNo.Y))
                typingImports.Add("List");

            if (decl.Elements.Any(e => e.Key != null))
                typingImports.Add("Union");

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
                    individualImports.Add("from datacentric.storage.typed_key import TypedKey");
                    individualImports.Add("from datacentric.storage.typed_record import TypedRecord");
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

            foreach (var key in decl.Elements.Where(d => d.Key != null).Select(d => d.Key))
            {
                if (PyExtensions.IsPackageEquals(decl, key))
                {
                    IDecl keyDecl = declarations.FindByKey(key);
                    individualImports.Add($"from {keyDecl.Category} import {key.Name}KeyHint");
                }
                else
                    packagesToImport.Add(PyExtensions.GetPackage(key));
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

            // Import date-time classes
            if (insideDc)
            {
                var atomicElements = decl.Elements
                                         .Where(e => e.Value != null)
                                         .GroupBy(e => e.Value.Type)
                                         .Select(g => g.First().Value.Type)
                                         .ToArray();

                if (atomicElements.Contains(AtomicType.DateTime) || atomicElements.Contains(AtomicType.NullableDateTime))
                    writer.AppendLine("from datacentric.date_time.local_date_time import LocalDateTimeHint");
                if (atomicElements.Contains(AtomicType.Date) || atomicElements.Contains(AtomicType.NullableDate))
                    writer.AppendLine("from datacentric.date_time.local_date import LocalDateHint");
                if (atomicElements.Contains(AtomicType.Time) || atomicElements.Contains(AtomicType.NullableTime))
                    writer.AppendLine("from datacentric.date_time.local_time import LocalTimeHint");
                if (atomicElements.Contains(AtomicType.Minute) || atomicElements.Contains(AtomicType.NullableMinute))
                    writer.AppendLine("from datacentric.date_time.local_minute import LocalMinuteHint");
                if (atomicElements.Contains(AtomicType.Instant) || atomicElements.Contains(AtomicType.NullableInstant))
                    writer.AppendLine("from datacentric.date_time.instant import InstantHint");
            }
        }
    }
}