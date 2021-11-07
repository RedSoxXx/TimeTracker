using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Common.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<T> GetItems<T>(bool includeNone = false)
            where T : struct
        {
            var enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException("enumType must be an enumeration type.");

            var result = Enum.GetValues(enumType);
            return includeNone ? result.Cast<T>() : result.Cast<int>().Where(i => i > 0 || Enum.GetName(enumType, i) != "None").Cast<T>();
        }

        public static IEnumerable<Enum> GetItems(Type enumType, bool includeNone = false)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException($"{nameof(enumType)} must be an enumeration type.");

            var result = Enum.GetValues(enumType);
            return includeNone ? result.Cast<Enum>() : result.Cast<Enum>().Where(i => i.AsInt() > 0 || Enum.GetName(enumType, i) != "None").ToArray();
        }

        public static Dictionary<string, string> ToDictionary(Type enumType, bool includeNone = false)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("enumType must be an enumeration type.");

            return Enum.GetValues(enumType).Cast<int>().Where(i => includeNone || i > 0 || Enum.GetName(enumType, i) != "None").ToDictionary(v => Enum.GetName(enumType, v), v => v.ToString());
        }

        public static Dictionary<string, string> ToDictionary<EnumType>(bool includeNone = false)
        {
            return ToDictionary(typeof(EnumType), includeNone);
        }

        public static TEnum ToggleFlag<TEnum>(this Enum self, TEnum flag, bool toggle)
            where TEnum : struct, IConvertible
        {
            if (!self.GetType().IsEquivalentTo(typeof(TEnum)))
                throw new ArgumentException("Enum value and flag types don't match.");

            return (TEnum)(ValueType)(toggle ?
                Convert.ToInt32(self) | Convert.ToInt32(flag) :
                Convert.ToInt32(self) & ~Convert.ToInt32(flag));
        }

        public static string GetDescription(this Enum self)
        {
            return self.GetAttribute<DescriptionAttribute>()?.Description ?? self.ToString().SplitUpperCase();
        }

        public static string GetDescription(Type enumType, int enumValue)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException($"{nameof(enumType)} must be an enumeration type.");

            if (!Enum.IsDefined(enumType, enumValue))
                throw new InvalidOperationException($"'{enumValue}' is not an underlying value of the '{enumType.Name}' enumeration.");

            return ((Enum)Enum.ToObject(enumType, enumValue)).GetDescription();
        }

        // http://stackoverflow.com/questions/5097766/how-to-get-custom-attribute-values-for-enums
        public static TAttribute GetAttribute<TAttribute>(this Enum self)
            where TAttribute : Attribute
        {
            var enumType = self.GetType();
            var memberInfo = enumType.GetMember(Enum.GetName(enumType, self)).FirstOrDefault();

            return memberInfo?.GetCustomAttributes<TAttribute>(false).FirstOrDefault();
        }

        public static bool HasAttribute<TAttribute>(this Enum self)
            where TAttribute : Attribute
        {
            return self.GetAttribute<TAttribute>() != null;
        }
    }
}
