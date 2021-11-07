using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Common.Extensions.Helpers;

namespace TimeTracker.Common.Json
{
    public static class JsonSerializerSettingsProvider
    {
        public static JsonSerializerSettings GetDefault(bool ignoreDefaults = false, bool? includeTypeInfo = null, bool prettify = false, bool enforceCamelCase = true)
        {
            var typeNameHandling = !includeTypeInfo.HasValue ? TypeNameHandling.Auto : (includeTypeInfo.Value ? TypeNameHandling.All : TypeNameHandling.None);

            var converters = new List<JsonConverter>(new[] { new StringEnumConverter() });
            if (typeNameHandling == TypeNameHandling.Auto)
                converters.Add(new SkipArrayTypeConverter());

            return new JsonSerializerSettings
            {
                ContractResolver = enforceCamelCase ? new CustomCamelCasePropertyNamesContractResolver() : new CustomDefaultContractResolver(),
                TypeNameHandling = typeNameHandling,
                DefaultValueHandling = ignoreDefaults ? DefaultValueHandling.Ignore : DefaultValueHandling.Include,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                Converters = converters,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = prettify ? Formatting.Indented : Formatting.None,
            };
        }

        public static void Merge(this JsonSerializerSettings targetSettings, JsonSerializerSettings sourceSettings)
        {
            targetSettings.ContractResolver = sourceSettings.ContractResolver;
            targetSettings.TypeNameHandling = sourceSettings.TypeNameHandling;
            targetSettings.Converters = sourceSettings.Converters;
            targetSettings.ReferenceLoopHandling = sourceSettings.ReferenceLoopHandling;
        }
    }
}
