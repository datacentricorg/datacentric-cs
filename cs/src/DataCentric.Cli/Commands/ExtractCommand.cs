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
using System.Reflection;
using CommandLine;

namespace DataCentric.Cli
{
    [Verb("extract", HelpText = "Extract type info from assemblies and convert to declarations.")]
    public class ExtractCommand
    {
        [Option('a', "assembly", HelpText = "Paths to assemblies to extract types.")]
        public IEnumerable<string> Assemblies { get; set; }

        [Value(0, MetaName = "types", HelpText = "Regex for types to filter.")]
        public IEnumerable<string> Types { get; set; }

        [Option('o', "output", HelpText = "Output folder to save generated declarations.", Default = ".")]
        public string OutputFolder { get; set; }

        [Option('p', "project", HelpText = "Path to search for corresponding project location.")]
        public string ProjectPath { get; set; }

        /// <summary>
        /// Corresponds to CLI "extract" keyword. Converts assembly types to declarations.
        /// ExtractVerbOptions.ProjectPath has been introduced to add project structure info to declarations.
        /// </summary>
        public void Execute()
        {
            AssemblyCache assemblies = new AssemblyCache();

            // Create list of assemblies (enrolling masks when needed)
            foreach (string assemblyPath in Assemblies)
            {
                string assemblyName = Path.GetFileName(assemblyPath);
                if (!string.IsNullOrEmpty(assemblyName))
                {
                    string assemblyDirectory =
                        string.IsNullOrEmpty(assemblyDirectory = Path.GetDirectoryName(assemblyPath)) ?
                        Environment.CurrentDirectory :
                        Path.GetFullPath(assemblyDirectory);
                    assemblies.AddFiles(Directory.EnumerateFiles(assemblyDirectory, assemblyName));
                }
            }

            // When no assemblies provided, search inside working directory
            if (assemblies.IsEmpty)
            {
                assemblies.AddFiles(Directory.EnumerateFiles(Environment.CurrentDirectory, "*.dll"));
            }

            Directory.CreateDirectory(OutputFolder);

            foreach (Assembly assembly in assemblies)
            {
                Console.Write("A> ");
                Console.WriteLine(assembly.Location);

                bool hasDocumentation = CommentNavigator.TryCreate(assembly, out CommentNavigator docNavigator);
                if (hasDocumentation)
                {
                    Console.Write("D> ");
                    Console.WriteLine(docNavigator.XmlLocation);
                }

                bool isProjectLocated = ProjectNavigator.TryCreate(ProjectPath, assembly, out ProjectNavigator projNavigator);
                if (isProjectLocated)
                {
                    Console.Write("P> ");
                    Console.WriteLine(projNavigator.Location);
                }

                List<Type> types = TypesExtractor.GetTypes(assembly, Types);

                foreach (Type type in types)
                {
                    TypeDecl decl = DeclarationConvertor.TypeToDecl(type, docNavigator, projNavigator);

                    string outputFolder = Path.Combine(OutputFolder, decl.Module.ModuleName.Replace('.','\\'));
                    Directory.CreateDirectory(outputFolder);

                    string extension = type.IsSubclassOf(typeof(Enum)) ? "clenum" : "cltype";
                    string outputFile = Path.Combine(outputFolder, $"{decl.Name}.{extension}");

                    Console.Write(type.FullName);
                    Console.Write(" => ");
                    Console.WriteLine(outputFile);

                    File.WriteAllText(outputFile, DeclarationSerializer.Serialize(decl));
                }

                List<Type> enums = TypesExtractor.GetEnums(assembly, Types);
                foreach (Type type in enums)
                {
                    EnumDecl decl = DeclarationConvertor.EnumToDecl(type, docNavigator, projNavigator);

                    string outputFolder = Path.Combine(OutputFolder, decl.Module.ModuleName.Replace('.','\\'));
                    Directory.CreateDirectory(outputFolder);

                    string extension = type.IsSubclassOf(typeof(Enum)) ? "clenum" : "cltype";
                    string outputFile = Path.Combine(outputFolder, $"{decl.Name}.{extension}");

                    Console.Write(type.FullName);
                    Console.Write(" => ");
                    Console.WriteLine(outputFile);

                    File.WriteAllText(outputFile, DeclarationSerializer.Serialize(decl));
                }
            }
        }
    }
}