using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Common.Extensions
{
    public static class ReflectionExtensions
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Storage = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(uint),
            typeof(double),
            typeof(float),
            typeof(decimal),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort)
        };

        public static bool IsSystemType(this Type self)
        {
            if (self == null)
                return false;

            return self.Namespace == nameof(System);
        }

        public static bool IsNumericType(this Type self)
        {
            if (self == null)
                return false;

            return NumericTypes.Contains(self) || NumericTypes.Contains(Nullable.GetUnderlyingType(self));
        }

        public static bool IsNullableType(this Type self)
        {
            if (self == null)
                return false;

            return self.IsGenericType && self.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool ImplementsInterface(this Type self, Type interfaceType)
        {
            if (self == null)
                return false;

            return self.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType ||
                    i == interfaceType);
        }

        public static bool IsEnumerable(this Type self)
        {
            if (self == null)
                return false;

            return !self.IsValueType && self != typeof(string) && typeof(IEnumerable).IsAssignableFrom(self);
        }

        public static bool IsGenericCollection(this Type self)
        {
            if (self == null)
                return false;

            return self.IsGenericType && self.GetGenericTypeDefinition() == typeof(ICollection<>);
        }

        public static bool IsAnonymous(this Type self)
        {
            if (self == null)
                return false;

            return Attribute.IsDefined(self, typeof(CompilerGeneratedAttribute), false) &&
                self.IsGenericType && self.Name.Contains("AnonymousType") &&
                (self.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || self.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase)) &&
                (self.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public static void SetAnonymousTypeProperty(this Type self, string propertyName, object targetObject, object value)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            if (propertyName.IsEmpty())
                throw new ArgumentNullException(nameof(propertyName));

            var field = self.GetField("<{0}>i__Field".FormatText(propertyName), BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
                throw new InvalidOperationException($"Failed to find underlying private field for property {propertyName}.");

            field.SetValue(targetObject, value);
        }

        public static string GetFullName(this Type self)
        {
            if (self == null)
                return null;

            return $"{self.FullName}, {self.Assembly.GetName().Name}";
        }

        public static string GetNamespaceWithoutAssemblyPart(this Type self)
        {
            if (self == null)
                throw new ArgumentNullException(nameof(self));

            return self.Namespace?.Replace(self.Assembly.GetName().Name + ".", "");
        }

        public static object GetDefault(this Type self)
        {
            if (self == null)
                return null;

            return self.IsValueType ? Activator.CreateInstance(self) : null;
        }

        public static IEnumerable<PropertyInfo> GetPublicInstanceProperties(this Type self)
        {
            if (self == null)
                return Enumerable.Empty<PropertyInfo>();

            return Storage.GetOrAdd(self, self.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        }

        public static PropertyInfo GetPublicInstanceProperty(this Type self, string name)
        {
            return self.GetPublicInstanceProperties().FirstOrDefault(p => p.Name == name);
        }

        public static IEnumerable<Assembly> GetBinAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(a =>
                !a.GlobalAssemblyCache &&
                    !(a is System.Reflection.Emit.AssemblyBuilder) &&
                    a.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder");
        }

        public static string GetExecutingAssemblyCodeBaseDir()
        {
            return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        }
    }
}
