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
using System.IO;
using System.Linq;
using System.Reflection;

namespace DataCentric
{
    [Configurable]
    public class Schema
    {
        private readonly IEnumerable<string> assemblies_ = new[] {""};
        private readonly string projectPath_ = null;

        public void Generate(Context context)
        {
            var assemblies = GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                bool hasDocumentation = CommentNavigator.TryCreate(assembly, out CommentNavigator docNavigator);
                bool isProjectLocated = ProjectNavigator.TryCreate(null, assembly, out ProjectNavigator projNavigator);

                var declTypes = ExtractTypes(assembly, docNavigator, projNavigator);
                var declEnums = ExtractEnums(assembly, docNavigator, projNavigator);

                context.SaveMany(declTypes);
                context.SaveMany(declEnums);
            }
        }

        private List<TypeDecl> ExtractTypes(Assembly assembly, 
            CommentNavigator docNavigator, ProjectNavigator projNavigator)
        {
            return TypesExtractor
                .GetTypes(assembly, new string[0])
                .Select(type => DeclarationConvertor.TypeToDecl(type, docNavigator, projNavigator))
                .ToList();
        }

        private List<EnumDecl> ExtractEnums(Assembly assembly, 
            CommentNavigator docNavigator, ProjectNavigator projNavigator)
        {
            return TypesExtractor
                .GetEnums(assembly, new string[0])
                .Select(type => DeclarationConvertor.EnumToDecl(type, docNavigator, projNavigator))
                .ToList();
        }
        
        private AssemblyCache GetAssemblies()
        {
            var assemblies = new AssemblyCache();

            foreach (var assemblyPath in assemblies_)
            {
                var assemblyName = Path.GetFileName(assemblyPath);

                if (string.IsNullOrEmpty(assemblyName)) 
                    continue;

                var assemblyDirectory = Path.GetDirectoryName(assemblyPath);

                assemblyDirectory = string.IsNullOrEmpty(assemblyDirectory) 
                    ? Environment.CurrentDirectory 
                    : Path.GetFullPath(assemblyDirectory);

                assemblies.AddFiles(Directory.EnumerateFiles(assemblyDirectory, assemblyName));
            }

            if (assemblies.IsEmpty)
                assemblies.AddFiles(Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll"));

            return assemblies;
        }

    }
}
