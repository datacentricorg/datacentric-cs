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
using System.IO;
using CommandLine;

namespace DataCentric.Cli
{
    [Verb("generate", HelpText = "Generate c++ header files from declarations.")]
    public class GenerateCommand
    {
        [Option('i', "input", Required = true, HelpText = "Path to declarations location.")]
        public string InputFolder { get; set; }

        [Option('o', "output", Required = true, HelpText = "Path to save generated c++ classes.")]
        public string OutputFolder { get; set; }

        [Option('s', "settings", HelpText = "File with settings for generator.")]
        public string SettingsPath { get; set; }

        /// <summary>
        /// Corresponds to CLI "generate" keyword. Converts given declarations to corresponding c++ files.
        /// </summary>
        public void Execute()
        {
            var declFiles = DeclConverter.ReadDeclUnits(InputFolder);

            GeneratorSettingsProvider.PopulateFromFile(SettingsPath);

            // Check Category field. In case if type name != file name it will be empty
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var decl in declFiles)
            {
                if (string.IsNullOrEmpty(decl.Category))
                    Console.WriteLine($"Warning! Unable to locate: {decl.Name}. Possible type<->file names mismatch.");
            }
            Console.ResetColor();

            var fileContentInfos = DeclConverter.ConvertSet(declFiles);

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