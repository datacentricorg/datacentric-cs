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
using CommandLine;

namespace DataCentric.Cli
{
    [Verb("headers", HelpText = "Generate c++ header files c# assemblies.")]
    public class HeadersCommand
    {
        [Option('a', "assembly", HelpText = "Paths to assemblies to extract types.")]
        public IEnumerable<string> Assemblies { get; set; }

        [Value(0, MetaName = "types", HelpText = "Regex for types to filter.")]
        public IEnumerable<string> Types { get; set; }

        [Option('p', "project", HelpText = "Path to search for corresponding project location.")]
        public string ProjectPath { get; set; }

        [Option('o', "output", HelpText = "Output folder to save generated c++ headers.", Default = ".")]
        public string OutputFolder { get; set; }

        [Option('s', "settings", HelpText = "File with settings for generator.")]
        public string SettingsPath { get; set; }

        /// <summary>
        /// Corresponds to CLI "headers" keyword. Converts given c# assemblies to corresponding c++ files.
        /// Combination of extract and generate keywords.
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
                    string assemblyDirectory = string.IsNullOrEmpty(assemblyDirectory = Path.GetDirectoryName(assemblyPath))
                                                   ? Environment.CurrentDirectory
                                                   : Path.GetFullPath(assemblyDirectory);
                    assemblies.AddFiles(Directory.EnumerateFiles(assemblyDirectory, assemblyName));
                }
            }

            List<IDecl> declarations = new List<IDecl>();
            foreach (Assembly assembly in assemblies)
            {
                CommentNavigator.TryCreate(assembly, out CommentNavigator docNavigator);
                ProjectNavigator.TryCreate(ProjectPath, assembly, out ProjectNavigator projNavigator);

                List<Type> types = TypesExtractor.GetTypes(assembly, Types);
                List<Type> enums = TypesExtractor.GetEnums(assembly, Types);
                declarations.AddRange(types.Concat(enums)
                                           .Select(type => DeclarationConvertor.ToDecl(type, docNavigator, projNavigator)));
            }

            GeneratorSettingsProvider.PopulateFromFile(SettingsPath);
            var fileContentInfos = DeclConverter.ConvertSet(declarations);
            foreach (var hppFile in fileContentInfos)
            {
                var fullPath = Path.Combine(OutputFolder, hppFile.FolderName, hppFile.FileName);
                var directory = Path.GetDirectoryName(fullPath);
                Directory.CreateDirectory(directory);

                if (File.Exists(fullPath))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"Warning! File already exists. Overwriting: {hppFile.FolderName}/{hppFile.FileName}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine($"Generated: {hppFile.FolderName}/{hppFile.FileName}");
                }

                File.WriteAllText(fullPath, hppFile.Content);
            }
        }
    }
}