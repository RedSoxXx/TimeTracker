using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TimeTracker.Common.Extensions.Helpers
{
    public class SkipArrayTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnumerable() && !typeof(IDictionary).IsAssignableFrom(objectType);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("ReadJson is not supported.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            if (serializer.TypeNameHandling != TypeNameHandling.Auto)
                throw new NotSupportedException("TypeNameHandling '{0}' is not supported.".FormatText(serializer.TypeNameHandling));

            bool? typeMismatchDetected = null;
            foreach (var item in (IEnumerable)value)
            {
                if (!typeMismatchDetected.HasValue)
                {
                    var elementType = value.GetType().GetElementType();
                    if (elementType == null)
                    {
                        var enumerableType = value.GetType().GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                        if (enumerableType != null)
                            elementType = enumerableType.GetGenericArguments().Single();
                    }

                    if (elementType == null || elementType.IsValueType)
                        typeMismatchDetected = false;
                    else
                        typeMismatchDetected = elementType != item.GetType();
                }

                if (typeMismatchDetected.Value)
                {
                    WriteItemWithType(writer, serializer, item);
                }
                else
                {
                    serializer.Serialize(writer, item);
                }
            }

            writer.WriteEndArray();
        }

        private void WriteItemWithType(JsonWriter writer, JsonSerializer serializer, object item)
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                serializer.Serialize(sw, item);

                string typeName;
                string assemblyName;
                serializer.Binder.BindToName(item.GetType(), out assemblyName, out typeName);

                sb = new StringBuilder(sb.ToString().TrimStart('{'));
                sb.Insert(0, String.Format("{{\"$type\":\"{0}, {1}\",", typeName, assemblyName.Split(',')[0]));
                writer.WriteRawValue(sb.ToString());
            }
        }
    }
}
