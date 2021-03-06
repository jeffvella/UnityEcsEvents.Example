﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Extensions
{
    public static class StringExtensions
    {
        public static string TrimStart(this string source, string value, StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase)
        {
            // #ref https://stackoverflow.com/questions/4335878/c-sharp-trimstart-with-string-parameter

            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int valueLength = value.Length;
            int startIndex = 0;

            while (source.IndexOf(value, startIndex, comparisonType) == startIndex)
                startIndex += valueLength;

            return source.Substring(startIndex);
        }
    }
}
