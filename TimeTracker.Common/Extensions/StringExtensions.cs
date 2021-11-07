using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TimeTracker.Common.Extensions
{
    public static partial class StringExtensions
    {
        public static bool IsEmpty(this string self)
        {
            return string.IsNullOrEmpty(self);
        }

        public static bool IsEmpty(this Guid self)
        {
            return self == Guid.Empty;
        }

        public static string[] SplitText(this string self, string symbol)
        {
            return self.IsEmpty() ? new string[] { } : self.Split(new[] { symbol }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitByNewLines(this string self)
        {
            return self.IsEmpty() ? Array.Empty<string>() : self.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string FormatText(this string self, params object[] args)
        {
            return string.Format(self, args);
        }

        public static bool EqualsIgnoringCase(this string self, string value)
        {
            if (self == null)
                return value == null;

            return self.Equals(value, StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool ContainsIgnoringCase(this string self, string value)
        {
            if (self == null)
                return value == null;

            return self.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string RemoveEmptySubstrings(this string self, params char[] separators)
        {
            if (self.IsEmpty())
                return self;

            var result = self;
            foreach (var separator in separators)
            {
                var substrings = result.Split(separator).Where(s => !s.Trim().IsEmpty());
                result = string.Join(separator.ToString(), substrings);
            }

            return result;
        }

        /// <summary>
        /// Parses a camel cased or pascal cased string and returns a new
        /// string with spaces between the words in the string.
        /// </summary>
        /// <example>
        /// The string "PascalCasing" will return an array with two
        /// elements, "Pascal" and "Casing".
        /// </example>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string SplitUpperCase(this string self)
        {
            return Regex.Replace(self, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        }

        public static string FirstLetterToLowerCase(this string self, string? separator = null)
        {
            if (self.IsEmpty())
                return null;

            self = self.Trim();
            if (self.Length == 1)
                return self.ToLower();

            if (!separator.IsEmpty())
                return string.Join(separator, self.SplitText(separator).Select(s => s.FirstLetterToLowerCase()));

            return Char.ToLower(self[0]) + self.Substring(1);
        }

        public static string? FirstLetterToUpperCase(this string self)
        {
            if (self.IsEmpty())
                return null;

            self = self.Trim();
            if (self.Length == 1)
                return self.ToUpper();

            return Char.ToUpper(self[0]) + self.Substring(1);
        }

        public static string TrimText(this string self, string symbols)
        {
            return self.IsEmpty() ? null : self.Trim(symbols.ToCharArray());
        }

        public static string TrimText(this string self, int maxLength)
        {
            if (self.IsEmpty())
                return null;

            const string endingSymbols = " ...";

            return self.Length <= maxLength ? self : self.Truncate(maxLength - endingSymbols.Length) + endingSymbols;
        }

        public static string Truncate(this string self, int maxLength)
        {
            if (self.IsEmpty())
                return null;

            return self.Length <= maxLength ? self : self.Substring(0, maxLength);
        }

        public static string ToCommaSeparated(this IEnumerable<int> self)
        {
            return (self ?? Enumerable.Empty<int>()).Select(v => v.ToString()).ToCommaSeparated();
        }

        public static string ToCommaSeparated(this IEnumerable<string> self)
        {
            return self.IsNullOrEmpty() ? string.Empty : string.Join(",", self);
        }

        public static string ToNewLineSeparated(this IEnumerable<string> self)
        {
            return self.IsNullOrEmpty() ? string.Empty : string.Join("\r\n", self);
        }

        public static IEnumerable<string> FromCommaSeparated(this string self)
        {
            return self.IsEmpty() ? Enumerable.Empty<string>() : self.Split(',');
        }

        public static string Remove(this string self, char[] chars)
        {
            return chars.Aggregate(self, (current, item) => current.Replace(item.ToString(), string.Empty));
        }

        public static string ToUtf8String(this byte[] self, bool removeEncodingInfo = false)
        {
            if (self == null || self.Length == 0)
                return null;

            var result = Encoding.UTF8.GetString(self);
            if (removeEncodingInfo)
                result = result.Replace(Encoding.UTF8.GetPreamble().ToUtf8String(), "");

            return result;
        }

        public static string ToAsciiString(this string self, string nonAsciiSymbolReplacement = "_")
        {
            if (self.IsEmpty())
                return self;

            var asciiEncoding = Encoding.GetEncoding(Encoding.ASCII.EncodingName,
                new EncoderReplacementFallback(nonAsciiSymbolReplacement),
                new DecoderExceptionFallback());

            return Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, asciiEncoding, Encoding.UTF8.GetBytes(self)));
        }

        public static byte[] ToBytes(this string self)
        {
            return self.IsEmpty() ? new byte[] { } : Encoding.UTF8.GetBytes(self);
        }

        public static string Shorten(this Guid self)
        {
            return Convert.ToBase64String(self.ToByteArray()).
                Replace("/", "x").
                Replace("+", "y").
                Replace("=", "").
                Substring(0, 22);
        }

        public static byte[] HexStringToBytes(this string self)
        {
            if (self.IsEmpty())
                return new byte[] { };

            if (self.StartsWith("0x"))
                self = self.Substring(2);

            return self.Replace("-", "").Batch(2).Select(e => byte.Parse(new string(e.ToArray()), System.Globalization.NumberStyles.HexNumber)).ToArray();
        }

        public static string BytesToHexString(this byte[] self)
        {
            if (self == null)
                return null;

            if (self.Length == 0)
                return "";

            return BitConverter.ToString(self).Replace("-", "");
        }

        #region - AsBool -

        public static bool AsBool(this object self, bool safe = false)
        {
            if (!safe)
            {
                if (self == null)
                    throw new ArgumentNullException(nameof(self));

                return Convert.ToBoolean(self);
            }

            if (self is string)
            {
                var str = (string)self;
                if (str.IsEmpty())
                    return false;

                bool value;
                return bool.TryParse(str, out value) && value;
            }

            try
            {
                return Convert.ToBoolean(self);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region - ToBase64 -

        public static string ToBase64(this string self, Encoding? encoding = null, bool safe = false)
        {
            encoding = encoding ?? Encoding.UTF8;
            if (!safe)
                return Convert.ToBase64String(encoding.GetBytes(self));

            if (self == null)
                return null;

            try
            {
                return Convert.ToBase64String(encoding.GetBytes(self));
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region - FromBase64 -

        public static string FromBase64(this string self, bool safe = false)
        {
            if (!safe)
            {
                if (self == null)
                    throw new ArgumentNullException(nameof(self));

                return Encoding.UTF8.GetString(Convert.FromBase64String(self));
            }

            if (self.IsEmpty())
                return null;

            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(self));
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region - AsDateTime -

        public static DateTime AsDateTime(this object self, bool safe = false)
        {
            if (!safe)
            {
                if (self == null)
                    throw new ArgumentNullException(nameof(self));

                return Convert.ToDateTime(self);
            }

            if (self is string)
            {
                var str = (string)self;
                if (str.IsEmpty())
                    return DateTime.MinValue;

                DateTime value;
                return !DateTime.TryParse(str, out value) ? DateTime.MinValue : value;
            }

            try
            {
                return Convert.ToDateTime(self);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime AsDateTime(this string self, string[] formats)
        {
            if (self.IsEmpty())
                throw new ArgumentNullException(nameof(self));

            if (formats.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(formats));

            return DateTime.ParseExact(self, formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        #endregion

        #region - AsGuid -

        public static Guid AsGuid(this object self, bool safe = false)
        {
            if (self is Guid guid)
                return guid;

            try
            {
                if (self is string s)
                    return new Guid(s);

                if (self is byte[] bytes)
                    return new Guid(bytes);
            }
            catch (Exception e)
            {
                if (!safe)
                    throw new ArgumentException("Input object is not a GUID.", e);
            }

            return Guid.Empty;
        }

        #endregion

        #region - AsInt -

        public static int AsInt(this object self, bool safe = false)
        {
            if (!safe)
            {
                if (self == null)
                    throw new ArgumentNullException(nameof(self));

                return Convert.ToInt32(self);
            }

            if (self is string)
            {
                int value;
                return !int.TryParse((string)self, out value) ? 0 : value;
            }

            try
            {
                return Convert.ToInt32(self);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region - AsLong -

        public static long AsLong(this object self, bool safe = false)
        {
            if (!safe)
            {
                if (self == null)
                    throw new ArgumentNullException(nameof(self));

                return Convert.ToInt64(self);
            }

            if (self is string)
            {
                long value;
                return !long.TryParse((string)self, out value) ? 0 : value;
            }

            try
            {
                return Convert.ToInt64(self);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region - AsDouble -

        public static double AsDouble(this object self, bool safe = false)
        {
            if (!safe)
            {
                if (self == null)
                    throw new ArgumentNullException(nameof(self));

                return Convert.ToDouble(self);
            }

            if (self is string)
            {
                double value;
                return !double.TryParse((string)self, out value) ? 0 : value;
            }

            try
            {
                return Convert.ToDouble(self);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region - AsFloat -

        public static float AsFloat(this object self, bool safe = false)
        {
            if (!safe)
            {
                if (self == null)
                    throw new ArgumentNullException(nameof(self));

                return Convert.ToSingle(self);
            }

            if (self is string)
            {
                float value;
                return !float.TryParse((string)self, out value) ? 0 : value;
            }

            try
            {
                return Convert.ToSingle(self);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region - AsDecimal -

        public static decimal AsDecimal(this object self, bool safe = false)
        {
            if (!safe)
            {
                if (self == null)
                    throw new ArgumentNullException(nameof(self));

                return Convert.ToDecimal(self);
            }

            if (self is string)
            {
                decimal value;
                return !decimal.TryParse((string)self, out value) ? 0 : value;
            }

            try
            {
                return Convert.ToDecimal(self);
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region - AsString -

        public static string AsString(this object self)
        {
            return Convert.ToString(self);
        }

        #endregion

        #region - AsEnum -

        public static T AsEnum<T>(this object self, bool safe = false)
        {
            if (self is int) return (T)self;

            if (!safe)
            {
                if (self == null)
                    throw new ArgumentNullException(nameof(self));

                return (T)Enum.Parse(typeof(T), (string)self, true);
            }

            try
            {
                return (T)Enum.Parse(typeof(T), (string)self, true);
            }
            catch
            {
                return default(T);
            }
        }

        #endregion

        #region - AsTimeSpan -

        /// <summary>
        /// Converts the string into TimeSpan value.
        /// </summary>
        /// <param name="self">The string to convert.</param>
        /// <returns>
        /// The converted value or the default value if the conversion can not be made.
        /// </returns>
        public static TimeSpan AsTimeSpan(this string self)
        {
            if (self.IsEmpty())
                return TimeSpan.Zero;

            var units = self.Trim().Where(char.IsLetter).ToArray();
            var value = self.TrimEnd(units);
            var intValue = AsInt(value);

            var format = new string(units);
            switch (format)
            {
                case "ms":
                    return TimeSpan.FromMilliseconds(intValue);
                case "s":
                    return TimeSpan.FromSeconds(intValue);
                case "m":
                    return TimeSpan.FromMinutes(intValue);
                case "h":
                    return TimeSpan.FromHours(intValue);
                default:
                    return TimeSpan.FromSeconds(intValue);
            }
        }

        #endregion
    }
}
