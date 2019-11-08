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

using Humanizer;

namespace DataCentric.Cli
{
    public static class PythonEnumBuilder
    {
        public static string Build(EnumDecl decl)
        {
            var writer = new CodeWriter();

            writer.AppendLine("from enum import Enum");

            writer.AppendNewLineWithoutIndent();
            writer.AppendNewLineWithoutIndent();


            writer.AppendLine($"class {decl.Name}(Enum):");
            writer.PushIndent();
            writer.AppendLines(CommentHelper.PyComment(decl.Comment));

            for (int index = 0; index < decl.Items.Count; index++)
            {
                EnumItemDecl item = decl.Items[index];

                writer.AppendLine($"{item.Name} = {index},");
                writer.AppendLines(CommentHelper.PyComment(item.Comment));

                // Do not add new line after last item
                if (index != decl.Items.Count - 1)
                    writer.AppendNewLineWithoutIndent();
            }

            writer.PopIndent();
            writer.AppendNewLineWithoutIndent();
            return writer.ToString();
        }
    }
}