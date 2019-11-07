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
using System.Text;
using System.Xml;
using System.Collections.Generic;

namespace DataCentric
{
    /// <summary>String wildcard pattern match.</summary>
    public class PatternMatch
    {
        private string pattern_;
        private string[] lowerCaseTokens_ = null;
        private string[] originalCaseTokens_ = null;

        /// <summary>Create from pattern which may contain *.</summary>
        public PatternMatch(string pattern)
        {
            pattern_ = pattern;

        }

        /// <summary>Match is by default case insensitive.</summary>
        public bool Match(string value)
        {
            if(lowerCaseTokens_ == null)
            {
                // Split pattern into tokens using *
                lowerCaseTokens_ = pattern_.ToLower().Split('*');
            }

            // Pattern is matched if each of the tokens is contained within the value
            value = value.ToLower();
            foreach(string token in lowerCaseTokens_)
            {
                if (!value.Contains(token)) return false;
            }
            return true;
        }

        /// <summary>Case sensitive match.</summary>
        public bool CaseSensitiveMatch(string value)
        {
            if (originalCaseTokens_ == null)
            {
                // Split pattern into tokens using *
                originalCaseTokens_ = pattern_.Split('*');
            }

            // Pattern is matched if each of the tokens is contained within the value
            foreach (string token in originalCaseTokens_)
            {
                if (!value.Contains(token)) return false;
            }
            return true;
        }
    }
}
