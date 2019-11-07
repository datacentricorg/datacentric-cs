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
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace DataCentric
{
    /// <summary>Text utilities.</summary>
    public static class TextUtil
    {
        const string alphabet_ = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Validates the format of pipe-delimited immutable name, and returns
        /// its components as (string, int) tuple.
        ///
        /// By convention, immutable name consists of two pipe delimited tokens.
        /// The first token is a string name prefix, and second token is integer
        /// name version with the initial value of 1. This method raises an
        /// error if the argument does not match the format.
        /// 
        /// For example, if the name is ABC|1, the returned tuple is (ABC,1).
        ///
        /// This method takes the identifier of parsed variable as nameOf parameter
        /// to use in error messages. The recommended way to pass the identifier is
        /// to use nameof(...) for the first argument.
        /// </summary>
        public static (string, int) ParseImmutableName(string nameOf, string name)
        {
            // Check that immutable name is not null or empty
            if (string.IsNullOrEmpty(name)) throw new Exception($"{nameOf} is not set.");

            // By convention, immutable name consists of two pipe delimited tokens.
            // The first token is a string identifier, and second token is integer
            // version number with the initial value of 1. This code raises an
            // error if the name does not match the format.
            string[] nameTokens = name.Split('|');
            if (nameTokens.Length != 2 ||
                !int.TryParse(nameTokens[1], out int nameVersion) ||
                nameVersion < 1)
            {
                throw new Exception(
                    $"Immutable {nameOf}={name} must consist of two pipe-delimited " +
                    $"tokens where  second token is a positive integer greater than zero, e.g. ABC|1.");
            }

            // If name is ABC|1, the returned tuple is (ABC,1)
            return (nameTokens[0], nameVersion);
        }

        /// <summary>
        /// Generate a random sequence of lowercase ASCII strings of specified length.
        /// This method generates pure alphabetical strings, without including numbers.
        /// 
        /// A total of stringLength strings with the length of stringLength will be
        /// generated.
        /// 
        /// No dictionary check is performed on the result to avoid unacceptable letter
        /// combinations; use with caution and always inspect the output.
        ///
        /// Change the value of random seed passed to this method to avoid generating
        /// the same sequence when this function is called multiple times. This method
        /// does not have cryptographic strength and should be used for test data generation
        /// only.
        /// </summary>
        public static List<string> GenerateRandomStrings(int stringCount, int stringLength, int randomSeed)
        {
            var result = new HashSet<string>();

            // Use seed when creating the random generator
            var rand = new Random(randomSeed);

            // Error message if the number of requested samples
            // is more than half of the total available number
            // of samples. For stringLength >= 5 this number
            // if high enough for most realistic stringCount
            // choices.
            if (stringLength < 5 && stringCount > Math.Pow(alphabet_.Length, stringLength) / 2)
                throw new Exception(
                    $"Requested number of random string samples {stringCount} " +
                    $"is too high for string length {stringLength}.");

            int sampleIndex = 0;
            while (sampleIndex < stringCount)
            {
                // Create sample string from random characters
                var builder = new StringBuilder();
                for (var j = 0; j < stringLength; j++)
                {
                    var c = alphabet_[rand.Next(0, alphabet_.Length)];
                    builder.Append(c);
                }
                string sample = builder.ToString();

                // Add if not yet present
                if (!result.Contains(sample))
                {
                    result.Add(sample);
                    sampleIndex++;
                }
            }

            return result.ToList();
        }
    }
}
