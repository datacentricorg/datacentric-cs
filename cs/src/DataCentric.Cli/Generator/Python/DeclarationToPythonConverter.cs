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

            var declPathDict = declarations.ToDictionary(GetNameKey, GetDeclModulePath);

            var types = typeDecls.Select(d => ConvertType(d, declPathDict));
            var enums = enumDecls.Select(ConvertEnum);

            return types.Concat(enums).ToList();
        }

        private static FileInfo ConvertType(TypeDecl decl, Dictionary<string, string> declPathDict)
        {
            var dataFile = new FileInfo
            {
                Content = PythonRecordBuilder.Build(decl, declPathDict).AppendCopyright(decl.Category),
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