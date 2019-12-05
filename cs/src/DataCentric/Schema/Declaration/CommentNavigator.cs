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
using System.Text;
using System.Xml.XPath;

namespace DataCentric
{
    /// <summary>
    /// Comment navigator class. Extracts comment info from XML file which is created alongside assembly.
    /// </summary>
    public class CommentNavigator
    {
        private readonly XPathNavigator navigator;

        /// <summary>
        /// Creates navigator instance from given XML document file.
        /// </summary>
        public CommentNavigator(string documentXmlLocation)
        {
            XmlLocation = documentXmlLocation;
            XPathDocument document = new XPathDocument(XmlLocation);
            navigator = document.CreateNavigator();
        }

        /// <summary>
        /// Helper method which tries to create documentation navigator for given assembly.
        /// </summary>
        public static bool TryCreate(Assembly assembly, out CommentNavigator navigator)
        {
            string documentFile = Path.ChangeExtension(assembly.Location, ".xml");
            if (File.Exists(documentFile))
            {
                navigator = new CommentNavigator(documentFile);
                return true;
            }
            navigator = null;
            return false;
        }

        /// <summary>
        /// Stores location for the documentation file of assembly.
        /// </summary>
        public string XmlLocation { get; }

        /// <summary>
        /// For given member extracts and returns formatted comment.
        /// </summary>
        public string GetXmlComment(MemberInfo member)
        {
            StringBuilder nameBuilder = new StringBuilder();
            if (member is System.Type type)
                nameBuilder.Append($"T:{type.FullName}");
            else if (member is MethodInfo method)
            {
                nameBuilder.Append($"M:{method.DeclaringType.FullName}.{method.Name}");

                ParameterInfo[] parameters = method.GetParameters();
                List<string> paramNames = parameters.Select(p => p.ParameterType.FullName).ToList();
                if (paramNames.Any())
                    nameBuilder.Append($"({string.Join(",", paramNames)})");
            }
            else if (member is PropertyInfo property)
                nameBuilder.Append($"P:{property.DeclaringType.FullName}.{property.Name}");
            else if (member is FieldInfo field)
                nameBuilder.Append($"F:{field.DeclaringType.FullName}.{field.Name}");
            else
                return null;

            string path = $"//doc//members//member[@name='{nameBuilder}']//summary";

            string value = navigator.SelectSingleNode(path)?.Value;
            if (value == null)
                return null;

            List<string> trimmed = value
                .Split(new[] {StringUtil.Eol}, StringSplitOptions.None)
                .Select(s => s.Trim(' ', '\t', '\r', '\n'))
                .ToList();

            return string.Join(StringUtil.Eol, trimmed).Trim(' ', '\t', '\r', '\n');
        }
    }
}