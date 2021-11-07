using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TimeTracker.Common.Extensions.Helpers
{
    public class CustomCamelCasePropertyNamesContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(MailMessage).IsAssignableFrom(objectType))
                return new MailMessageJsonConverter();

            return base.ResolveContractConverter(objectType);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            JsonConverter converter = null;

            if (typeof(MailMessage).IsAssignableFrom(property.PropertyType))
                converter = new MailMessageJsonConverter();

            if (converter != null)
                property.Converter = converter;

            return property;
        }
    }
}
