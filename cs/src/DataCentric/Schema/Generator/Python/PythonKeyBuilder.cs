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

namespace DataCentric.Cli
{
    /// <summary>
    /// Builder for generated python keys.
    /// </summary>
    public static class PythonKeyBuilder
    {
        /// <summary>
        /// Generate python classes from declaration.
        /// </summary>
        public static string Build(TypeDecl decl)
        {
            // Determine if we are inside datacentric package
            // based on module name. This affects the imports
            // and namespace use.
            bool insideDc = PyExtensions.GetPackage(decl) == "datacentric";

            // If not generating for DataCentric package, use dc. namespace
            // in front of datacentric types, otherwise use no prefix
            string dcNamespacePrefix = insideDc ? "" : "dc.";

            var writer = new CodeWriter();
            writer.AppendLine("from abc import ABC");

            string keyImport = insideDc
                ? "from datacentric.storage.key import Key"
                : "import datacentric as dc";
            writer.AppendLine(keyImport);

            writer.AppendNewLineWithoutIndent();
            writer.AppendNewLineWithoutIndent();

            writer.AppendLine($"class {decl.Name}Key({dcNamespacePrefix}Key, ABC):");
            writer.PushIndent();
            writer.AppendLines(CommentHelper.PyComment(decl.Comment));
            writer.AppendLine("pass");
            writer.PopIndent();

            return writer.ToString();
        }
    }
}