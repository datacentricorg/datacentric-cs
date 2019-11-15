/*
Copyright (C) 2003-present CompatibL. All rights reserved.

This code contains valuable trade secrets and may be downloaded, copied, stored,
used, or distributed only in compliance with the terms of a written commercial
license from CompatibL and with the inclusion of this copyright notice.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace DataCentric.Cli.Test
{
    /// <summary>
    /// Test generation of Python source code.
    ///
    /// This unit test is not derived from UnitTest record because it
    /// uses CallerFilePath and cannot be invoked as a Handler.
    /// </summary>
    public class PythonGeneratorTest
    {
        /// <summary>Smoke test for the generation of Python source code.</summary>
        [Fact]
        public void Smoke()
        {
            using (var context = new TemporalMongoUnitTestContext(this))
            {
                // Root of the C# source code of the DataCentric module
                string testFolder = Path.GetDirectoryName(context.CallerFilePath);
                string srcRootFolder = Path.Combine(testFolder, "..\\..\\..");
                string libFolder = Path.Combine(srcRootFolder, "DataCentric.Test\\bin\\Debug\\netcoreapp2.1"); // TODO - support Linux and release modes
                var options = new ExtractCommand
                {
                    Assemblies = Directory.GetFiles(libFolder, "*.dll"),
                    OutputFolder = testFolder,
                    Types = new List<string>(),
                    ProjectPath = srcRootFolder
                };

                AssemblyCache assemblies = new AssemblyCache();

                // Create list of assemblies (enrolling masks when needed)
                foreach (string assemblyPath in options.Assemblies)
                {
                    string assemblyName = Path.GetFileName(assemblyPath);
                    if (!string.IsNullOrEmpty(assemblyName))
                    {
                        string assemblyDirectory =
                            string.IsNullOrEmpty(assemblyDirectory = Path.GetDirectoryName(assemblyPath))
                                ? Environment.CurrentDirectory
                                : Path.GetFullPath(assemblyDirectory);
                        assemblies.AddFiles(Directory.EnumerateFiles(assemblyDirectory, assemblyName));
                    }
                }

                List<IDecl> declarations = new List<IDecl>();

                foreach (Assembly assembly in assemblies)
                {
                    CommentNavigator.TryCreate(assembly, out CommentNavigator docNavigator);
                    ProjectNavigator.TryCreate(options.ProjectPath, assembly, out ProjectNavigator projNavigator);

                    List<Type> types = TypesExtractor.GetTypes(assembly, options.Types);
                    List<Type> enums = TypesExtractor.GetEnums(assembly, options.Types);
                    declarations.AddRange(types.Concat(enums)
                        .Select(type => DeclarationConvertor.ToDecl(type, docNavigator, projNavigator)));
                }

                foreach (var decl in declarations)
                {
                    if (decl.Category != null && decl.Category.StartsWith("DataCentric"))
                    {
                        decl.Category = decl.Category.TrimStart("DataCentric");
                        decl.Category = "Datacentric" + decl.Category;
                    }
                }

                var converted = DeclarationToPythonConverter.ConvertSet(declarations);

                var result = new Dictionary<string, string>();
                foreach (var file in converted)
                    if (file.FolderName == null)
                        result[$"{file.FileName}"] = file.Content;
                    else
                        result[$"{file.FolderName}/{file.FileName}"] = file.Content;

                // Record the contents of a selected list of generated files
                // for the purposes of approval testing
                var approvalTestFiles = new List<string>
                {
                    "job_status",
                    "job",
                    "job_key",
                    "zone",
                    "zone_key"
                };

                // Record approval test output
                foreach (var file in result.OrderBy(f => f.Key))
                {
                    string[] tokens = file.Key.Split('/');
                    string fileName = tokens[tokens.Length - 1].Split('.')[0];

                    if (approvalTestFiles.Contains(fileName))
                    {
                        context.Log.Verify(file.Key, file.Value);
                    }
                }

                // Enable to generate files on disk
                if (false)
                {
                    foreach (var file in result)
                    {
                        var fullPath = Path.Combine(options.OutputFolder, file.Key);
                        var directory = Path.GetDirectoryName(fullPath);
                        Directory.CreateDirectory(directory);

                        File.WriteAllText(fullPath, file.Value);
                    }
                }
            }
        }
    }
}