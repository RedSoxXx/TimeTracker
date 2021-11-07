using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimeTracker.Common.Extensions.Helpers
{
    public class MailMessageJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(MailMessage);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            var customSerializer = new JsonSerializer();
            customSerializer.Converters.Add(new MailAddressJsonConverter());
            customSerializer.Converters.Add(new NameValueCollectionConverter());
            customSerializer.Converters.Add(new EncodingConverter());

            var mailMessage = (MailMessage)value;

            if (mailMessage.Attachments.Any() || mailMessage.AlternateViews.Any())
                throw new JsonSerializationException($"Cant serialize {typeof(MailMessage).FullName} with Attachments or AlternateViews.");

            JToken.FromObject(mailMessage, customSerializer).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Null)
                return null;

            if (token.Type != JTokenType.Object)
                throw new JsonSerializationException($"Unexpected token type: {token.Type}");

            var customSerializer = new JsonSerializer();
            customSerializer.Converters.Add(new MailAddressJsonConverter());
            customSerializer.Converters.Add(new NameValueCollectionConverter());
            customSerializer.Converters.Add(new EncodingConverter());

            var message = token.ToObject<MailMessage>(customSerializer);

            var mailMessageHeaders = token["Headers"]?.ToObject<NameValueCollection>(customSerializer) ?? token["headers"]?.ToObject<NameValueCollection>(customSerializer);

            if (mailMessageHeaders != null)
            {
                foreach (var key in mailMessageHeaders.AllKeys)
                {
                    message.Headers.Add(key, mailMessageHeaders[key]);
                }
            }

            return message;
        }

        private class EncodingConverter : JsonConverter
        {
            private readonly IReadOnlyDictionary<string, string> _friendlyEncodingNamesMap = new Dictionary<string, string>
            {
                ["Unicode (UTF-8)"] = "UTF-8",
            };

            public override bool CanConvert(Type objectType)
            {
                return typeof(Encoding).IsAssignableFrom(objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (writer == null)
                    throw new ArgumentNullException(nameof(writer));
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (serializer == null)
                    throw new ArgumentNullException(nameof(serializer));

                JToken.FromObject(((Encoding)value).EncodingName).WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader == null)
                    throw new ArgumentNullException(nameof(reader));

                var token = JToken.Load(reader);

                if (token.Type == JTokenType.Null)
                    return null;

                string encodingName;

                if (token.Type == JTokenType.String)
                    encodingName = token.ToObject<string>();
                else
                    throw new JsonSerializationException($"Unexpected token type: {token.Type}");

                try
                {
                    return Encoding.GetEncoding(encodingName);
                }
                catch (Exception)
                {
                    if (_friendlyEncodingNamesMap.TryGetValue(encodingName, out encodingName))
                        return Encoding.GetEncoding(encodingName);

                    throw;
                }
            }
        }

        private class NameValueCollectionConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(NameValueCollection).IsAssignableFrom(objectType);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (writer == null)
                    throw new ArgumentNullException(nameof(writer));
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (serializer == null)
                    throw new ArgumentNullException(nameof(serializer));

                var nameValueCollection = (NameValueCollection)value;
                JToken.FromObject(nameValueCollection.AllKeys.Select(k => new KeyValuePair<string, string>(k, nameValueCollection[k]))).WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader == null)
                    throw new ArgumentNullException(nameof(reader));

                var token = JToken.Load(reader);

                if (token.Type == JTokenType.Null)
                    return null;

                if (token.Type != JTokenType.Array)
                    throw new JsonSerializationException($"Unexpected token type: {token.Type}");

                var nameValueCollection = new NameValueCollection();

                foreach (var keyValuePair in token.ToObject<IEnumerable<KeyValuePair<string, string>>>())
                {
                    nameValueCollection.Add(keyValuePair.Key, keyValuePair.Value);
                }

                return nameValueCollection;
            }
        }

        private class MailAddressJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(MailAddress);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (writer == null)
                    throw new ArgumentNullException(nameof(writer));
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (serializer == null)
                    throw new ArgumentNullException(nameof(serializer));

                JToken.FromObject(value).WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader == null)
                    throw new ArgumentNullException(nameof(reader));

                var token = JToken.Load(reader);

                if (token.Type == JTokenType.Null)
                    return null;

                if (token.Type != JTokenType.Object)
                    throw new JsonSerializationException($"Unexpected token type: {token.Type}");

                var address = token["Address"]?.Value<string>() ?? token["address"]?.Value<string>();
                var displayName = token["DisplayName"]?.Value<string>() ?? token["displayName"]?.Value<string>();

                if (displayName.IsEmpty())
                    return new MailAddress(address);

                return new MailAddress(address, displayName);
            }
        }
    }
}