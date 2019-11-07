using System;
using System.Collections.Generic;

namespace DataCentric
{
    // TODO: Replace strings with spans in netstandard2.1
    // TODO: Implement space trimming
    public static class ParamKeyValueParser
    {
        private static bool TryParsePair(
            string source, int startIndex, int count,
            char valueDelimiter,
            out string key, out string value)
        {
            int valueIndex = source.IndexOf(valueDelimiter, startIndex, count);

            if (valueIndex > 0)
            {
                int keyLength = valueIndex - startIndex;
                key = source.Substring(startIndex, keyLength);
                value = source.Substring(valueIndex + 1, count - keyLength - 1);
                return true;
            }

            key = null;
            value = null;
            return false;
        }

        private static KeyValuePair<string, string> ParsePair(
            string source, int startIndex, int count,
            char valueDelimiter)
        {
            if (TryParsePair(source, startIndex, count, valueDelimiter, out string key, out string value))
            {
                return new KeyValuePair<string, string>(key, value);
            }
            throw new ArgumentException($"'{source.Substring(startIndex, count)}' is not valid key{valueDelimiter}value pair");
        }

        public static bool TryParsePair(string source, out string key, out string value)
        {
            return TryParsePair(
                source ?? throw new ArgumentNullException(nameof(source)),
                0, source.Length,
                DefaultValueDelimiter,
                out key, out value);
        }

        public static KeyValuePair<string, string> ParsePair(string source,
            char valueDelimiter = DefaultValueDelimiter)
        {
            return ParsePair(
                source ?? throw new ArgumentNullException(nameof(source)),
                0, source.Length,
                valueDelimiter);
        }

        public static IEnumerable<KeyValuePair<string, string>> Parse(string source,
            char pairDelimiter = DefaultPairDelimiter, char valueDelimiter = DefaultValueDelimiter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            int currentIndex = 0;
            int nextIndex;

            while ((nextIndex = source.IndexOf(pairDelimiter, currentIndex)) >= 0)
            {
                if (nextIndex > currentIndex)
                {
                    yield return ParsePair(source, currentIndex, nextIndex - currentIndex, valueDelimiter);
                }
                currentIndex = nextIndex + 1;
            }

            int lastLength = source.Length - currentIndex;
            if (lastLength > 0)
            {
                yield return ParsePair(source, currentIndex, lastLength, valueDelimiter);
            }
        }

        public const char DefaultPairDelimiter = ',';
        public const char DefaultValueDelimiter = '=';
    }
}
