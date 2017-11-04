using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace PortalBot.Models
{

    public class Fact
    {
        [JsonProperty(PropertyName = "n")]
        public string Number { get; set; }

        [JsonProperty(PropertyName = "d")]
        public string Data { get; set; }

        [JsonProperty(PropertyName = "c")]
        [JsonConverter(typeof(SingleOrArrayConverter<string>))]
        public List<string> Categories { get; set; }
    }

    internal class SingleOrArrayConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(List<T>));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            return token.Type == JTokenType.Array ? token.ToObject<List<T>>() : new List<T> { token.ToObject<T>() };
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
