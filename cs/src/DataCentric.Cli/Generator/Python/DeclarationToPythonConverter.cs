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
    public static class DeclarationToPythonConverter
    {
        public static List<FileInfo> ConvertSet(List<IDecl> declarations)
        {
            declarations = declarations.Where(t => t.Module.ModuleName.StartsWith("DataCentric")).ToList();
            List<TypeDecl> typeDecls = declarations.OfType<TypeDecl>().ToList();
            List<EnumDecl> enumDecls = declarations.OfType<EnumDecl>().ToList();

            // Construct dictionary for declaration and corresponding module
            string GetNameKey(IDecl v) => v.Module.ModuleName + "." + v.Name;
            string GetDeclModulePath(IDecl decl) => $"{decl.Category?.Underscore()}.{decl.Name.Underscore()}";

            var declModuleDict = declarations.ToDictionary(GetNameKey, GetDeclModulePath);

            var types = typeDecls.Select(d => ConvertType(d, declModuleDict));
            var enums = enumDecls.Select(ConvertEnum);
            var init = GenerateInitFiles(declarations, declModuleDict);

            return types.Concat(enums).Concat(init).ToList();
        }

        private static List<FileInfo> GenerateInitFiles(List<IDecl> declarations, Dictionary<string, string> declModuleDict)
        {
            Dictionary<string, List<string>> packageImports = new Dictionary<string, List<string>>();

            List<TypeDecl> typeDecls = declarations.OfType<TypeDecl>().ToList();
            List<EnumDecl> enumDecls = declarations.OfType<EnumDecl>().ToList();

            foreach (var decl in enumDecls)
            {
                string moduleImport = declModuleDict[decl.Module.ModuleName + "." + decl.Name];
                int indexOf = moduleImport.IndexOf('.');
                var package = moduleImport.Substring(0, indexOf);
                if (!packageImports.ContainsKey(package))
                    packageImports[package] = new List<string>();
                packageImports[package].Add($"from {moduleImport} import {decl.Name}");
            }

            foreach (var decl in typeDecls)
            {
                string moduleImport = declModuleDict[decl.Module.ModuleName + "." + decl.Name];
                int indexOf = moduleImport.IndexOf('.');
                var package = moduleImport.Substring(0, indexOf);
                if (!packageImports.ContainsKey(package))
                    packageImports[package] = new List<string>();
                if (decl.IsRecord && decl.Inherit == null)
                    packageImports[package].Add($"from {moduleImport} import {decl.Name}, {decl.Name}Key");
                else
                    packageImports[package].Add($"from {moduleImport} import {decl.Name}");
            }

            var result = new List<FileInfo>();
            foreach (var pair in packageImports)
            {
                var init = new FileInfo
                {
                    FileName = "__init__.py",
                    FolderName = pair.Key,
                    Content = string.Join(Environment.NewLine, pair.Value)
                };
                result.Add(init);
            }

            return result;
        }

        private static FileInfo ConvertType(TypeDecl decl, Dictionary<string, string> declModuleDict)
        {
            var dataFile = new FileInfo
            {
                Content = PythonRecordBuilder.Build(decl, declModuleDict).AppendCopyright(decl.Category),
                FileName = $"{decl.Name.Underscore()}.py",
                FolderName = decl.Category?.Underscore().Replace('.', '/')
            };

            return dataFile;
        }

        private static FileInfo ConvertEnum(EnumDecl decl)
        {
            var enumFile = new FileInfo
            {
                Content = PythonEnumBuilder.Build(decl).AppendCopyright(decl.Category),
                FileName = $"{decl.Name.Underscore()}.py",
                FolderName = decl.Category?.Underscore().Replace('.', '/')
            };

            return enumFile;
        }

        private static string AppendCopyright(this string input, string category)
        {
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

            if (category == null)
            {
                return "# Copyright not specified";
            }
            else if (category.StartsWith("Datacentric"))
            {
                return dcCopyright + input;
            }
            else throw new Exception($"Copyright header is not specified for module {category}.");
        }
    }
}