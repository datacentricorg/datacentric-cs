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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Humanizer;

namespace DataCentric.Cli
{
    public class FileInfo
    {
        public string Content { get; set; }
        public string FileName { get; set; }
        public string FolderName { get; set; }
    }

    public static class DeclConverter
    {
        public static List<IDecl> ReadDeclUnits(string path)
        {
            var result = new List<IDecl>();

            var typeFiles = Directory.GetFiles(path, "*.cltype", SearchOption.AllDirectories);
            foreach (var file in typeFiles)
            {
                string text = File.ReadAllText(file);
                result.Add(DeclarationSerializer.Deserialize<TypeDecl>(text));
            }

            var enumFiles = Directory.GetFiles(path, "*.clenum", SearchOption.AllDirectories);
            foreach (var file in enumFiles)
            {
                string text = File.ReadAllText(file);
                result.Add(DeclarationSerializer.Deserialize<EnumDecl>(text));
            }

            return result;
        }

        public static List<FileInfo> ConvertSet(List<IDecl> declarations)
        {
            List<TypeDecl> typeDecls = declarations.OfType<TypeDecl>().ToList();
            List<EnumDecl> enumDecls = declarations.OfType<EnumDecl>().ToList();

            var typeIncludes = typeDecls.ToDictionary(t => t.Name, GetIncludePath);
            var enumIncludes = enumDecls.ToDictionary(t => t.Name, GetIncludePath);

            var includes = typeIncludes.Concat(enumIncludes).ToDictionary(t => t.Key, t => t.Value);

            var types = typeDecls.SelectMany(d => ConvertType(d, includes));
            var enums = enumDecls.SelectMany(d => ConvertEnum(d, includes));

            return types.Concat(enums).ToList();
        }

        private static string GetIncludePath(IDecl decl)
        {
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);

            // Avoid adding trailing path separator
            return !string.IsNullOrEmpty(decl.Category) && !decl.Category.Equals(".")
                       ? $"{settings.Namespace}.{decl.Category}".Underscore().Replace('.', '/')
                       : $"{settings.Namespace}".Underscore().Replace('.', '/');
        }

        private static List<FileInfo> ConvertType(TypeDecl decl, Dictionary<string, string> includePath)
        {
            string pathInProject = includePath[decl.Name];

            List<FileInfo> result = new List<FileInfo>();

            var dataHeader = new FileInfo
            {
                Content = CppDataBuilder.BuildDataHeader(decl, includePath),
                FileName = $"{decl.Name.Underscore()}_data.hpp",
                FolderName = pathInProject
            };
            result.Add(dataHeader);

            var dataSource = new FileInfo
            {
                Content = CppDataBuilder.BuildDataSource(decl, includePath),
                FileName = $"{decl.Name.Underscore()}_data.cpp",
                FolderName = pathInProject
            };
            result.Add(dataSource);

            if (decl.Keys.Any())
            {
                var keyHeader = new FileInfo
                {
                    Content = CppKeyBuilder.BuildKeyHeader(decl, includePath),
                    FileName = $"{decl.Name.Underscore()}_key.hpp",
                    FolderName = pathInProject
                };
                result.Add(keyHeader);

                var keySource = new FileInfo
                {
                    Content = CppKeyBuilder.BuildKeySource(decl, includePath),
                    FileName = $"{decl.Name.Underscore()}_key.cpp",
                    FolderName = pathInProject
                };
                result.Add(keySource);
            }

            return result;
        }

        private static List<FileInfo> ConvertEnum(EnumDecl decl, Dictionary<string, string> includePath)
        {
            var result = new List<FileInfo>();
            var settings = GeneratorSettingsProvider.Get(decl.Module.ModuleName);
            string folderName = $"{settings.Namespace}.{decl.Category}".Underscore().Replace('.', '/');

            var enumHeader = new FileInfo
            {
                Content = CppEnumBuilder.BuildEnumHeader(decl, includePath),
                FileName = $"{decl.Name.Underscore()}.hpp",
                FolderName = folderName
            };
            result.Add(enumHeader);

            var enumSource = new FileInfo
            {
                Content = CppEnumBuilder.BuildEnumSource(decl, includePath),
                FileName = $"{decl.Name.Underscore()}.cpp",
                FolderName = folderName
            };
            result.Add(enumSource);

            return result;
        }
    }
}