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

namespace DataCentric
{
    /// <summary>
    /// Builder for generated python enum.
    /// </summary>
    public static class PythonEnumBuilder
    {
        /// <summary>
        /// Generate python enum from declaration.
        /// </summary>
        public static string Build(EnumDecl decl)
        {
            var writer = new CodeWriter();

            writer.AppendLine("from enum import IntEnum");

            writer.AppendNewLineWithoutIndent();
            writer.AppendNewLineWithoutIndent();


            writer.AppendLine($"class {decl.Name}(IntEnum):");
            writer.PushIndent();
            writer.AppendLines(CommentHelper.PyComment(decl.Comment));
            writer.AppendNewLineWithoutIndent();

            var items = decl.Items;
            for (int index = 0; index < items.Count; index++)
            {
                EnumItemDecl item = items[index];

                writer.AppendLine($"{item.Name} = {index},");
                writer.AppendLines(CommentHelper.PyComment(item.Comment));

                // Do not add new line after last item
                if (index != items.Count - 1)
                    writer.AppendNewLineWithoutIndent();
            }

            writer.PopIndent();
            return writer.ToString();
        }
    }
}