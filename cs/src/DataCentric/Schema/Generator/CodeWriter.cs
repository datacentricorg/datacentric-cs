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
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace DataCentric
{
    public class CodeWriter
    {
        private readonly IndentedTextWriter writer;

        public CodeWriter(string tabString = "    ")
        {
            writer = new IndentedTextWriter(new StringWriter(new StringBuilder()), tabString);
        }

        public void Append(string text)
        {
            writer.Write(text);
        }

        public void AppendLine(string text)
        {
            writer.WriteLine(text);
        }

        public void AppendLines(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            var lines = text.Split(new[] { StringUtil.Eol }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                // Do not add spaces for empty line
                if (string.IsNullOrEmpty(line))
                    writer.WriteLineNoTabs("");
                else
                    writer.WriteLine(line);
            }
        }

        public void AppendLine()
        {
            writer.WriteLine();
        }

        public void AppendNewLineWithoutIndent()
        {
            writer.WriteLineNoTabs("");
        }

        public override string ToString()
        {
            return writer.InnerWriter.ToString();
        }

        public void PushIndent()
        {
            writer.Indent++;
        }

        public void PopIndent()
        {
            writer.Indent--;
        }
    }
}