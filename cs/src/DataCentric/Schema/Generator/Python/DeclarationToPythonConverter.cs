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
using DataCentric.Schema.Declaration.Type;
using Humanizer;

namespace DataCentric.Schema.Generator.Python
{
    /// <summary>
    /// Converts declarations to generated python files.
    /// </summary>
    public static class DeclarationToPythonConverter
    {
        /// <summary>
        /// Perform conversion.
        /// </summary>
        public static List<FileInfo> ConvertSet(List<IDecl> declarations)
        {
            declarations = declarations.Where(t => t.Module.ModuleName.StartsWith("DataCentric")).ToList();

            // Skipping declarations with empty Category, since it is impossible
            // to determine their proper package and module in python.
            var skipped = declarations.Where(d => string.IsNullOrEmpty(d.Category)).ToList();
            foreach (var decl in skipped)
            {
                Console.WriteLine($"Skipping {decl.Module.ModuleName}:{decl.Name} since filename was not resolved. " +
                                  $"Possible class-filename mismatch or duplicate file names.");
            }

            // From now - Category is guaranteed to be not null and non-empty.
            declarations = declarations.Except(skipped).ToList();

            // Modify DataCentric to avoid its conversion to data_centric by Underscore()
            foreach (var decl in declarations)
            {
                if (decl.Category.StartsWith("DataCentric"))
                {
                    decl.Category = decl.Category.TrimStart("DataCentric");
                    decl.Category = "Datacentric" + decl.Category;
                }
            }

            // !!! Important.
            // Convert information to usable form, namely:
            // Category to python module.
            foreach (var decl in declarations)
            {
                // e.g. datacentric.storage.context
                var module = decl.Category.Underscore() + '.' + decl.Name.Underscore();
                decl.Category = module;
            }

            List<TypeDecl> typeDecls = declarations.OfType<TypeDecl>().ToList();
            List<EnumDecl> enumDecls = declarations.OfType<EnumDecl>().ToList();

            var types = typeDecls.Select(d => ConvertType(d, declarations));
            var enums = enumDecls.Select(ConvertEnum);
            var init = GenerateInitFiles(declarations);

            return types.Concat(enums).Concat(init).ToList();
        }

        private static List<FileInfo> GenerateInitFiles(List<IDecl> declarations)
        {
            Dictionary<string, List<string>> packageImports = new Dictionary<string, List<string>>();

            List<TypeDecl> typeDecls = declarations.OfType<TypeDecl>().ToList();
            List<EnumDecl> enumDecls = declarations.OfType<EnumDecl>().ToList();

            foreach (var decl in enumDecls)
            {
                var package = PyExtensions.GetPackage(decl);
                if (!packageImports.ContainsKey(package))
                    packageImports[package] = new List<string>();

                packageImports[package].Add($"from {decl.Category} import {decl.Name}");
            }

            foreach (var decl in typeDecls)
            {
                var package = PyExtensions.GetPackage(decl);
                if (!packageImports.ContainsKey(package))
                    packageImports[package] = new List<string>();

                // Two classes are imported in case of first children of record
                if (decl.IsRecord && decl.Inherit == null)
                    packageImports[package].Add($"from {decl.Category} import {decl.Name}, {decl.Name}Key");
                else
                    packageImports[package].Add($"from {decl.Category} import {decl.Name}");
            }

            var result = new List<FileInfo>();
            foreach (var pair in packageImports)
            {
                var init = new FileInfo
                {
                    FileName = "__init__.py",
                    FolderName = pair.Key,
                    Content = string.Join(StringUtil.Eol, pair.Value)
                };
                result.Add(init);
            }

            return result;
        }

        private static FileInfo ConvertType(TypeDecl decl, List<IDecl> declarations)
        {
            // Decompose package to folder and file name.
            int dotIndex = decl.Category.LastIndexOf('.');
            string fileName = $"{decl.Category.Substring(dotIndex+1)}.py";
            string folderName = decl.Category.Substring(0,dotIndex).Replace('.', '/');

            var dataFile = new FileInfo
            {
                Content = PythonRecordBuilder.Build(decl, declarations).AppendCopyright(decl),
                FileName = fileName,
                FolderName = folderName
            };

            return dataFile;
        }

        private static FileInfo ConvertEnum(EnumDecl decl)
        {
            // Decompose package to folder and file name.
            int dotIndex = decl.Category.LastIndexOf('.');
            string fileName = $"{decl.Category.Substring(dotIndex+1)}.py";
            string folderName = decl.Category.Substring(0,dotIndex).Replace('.', '/');

            var enumFile = new FileInfo
            {
                Content = PythonEnumBuilder.Build(decl).AppendCopyright(decl),
                FileName = fileName,
                FolderName = folderName
            };

            return enumFile;
        }

        private static string AppendCopyright(this string input, IDecl declaration)
        {
            string package = PyExtensions.GetPackage(declaration);

            string dcCopyright = @"# Copyright (C) 2013-present The DataCentric Authors.
#
# Licensed under the Apache License, Version 2.0 (the ""License"");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#    http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an ""AS IS"" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

";

            if (package == "datacentric")
            {
                return dcCopyright + input;
            }
            else throw new Exception($"Copyright header is not specified for module {package}.");
        }
    }
}