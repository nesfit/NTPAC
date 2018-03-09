using System;
using System.Net;
using Newtonsoft.Json;

namespace NTPAC.ApplicationProtocolExport.Core.Serializers.Converters
{
  internal class IPAddressConverter : JsonConverter
  {
    public override Boolean CanConvert(Type objectType) => objectType == typeof(IPAddress);

    public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer) =>
      writer.WriteValue(value.ToString());

    public override Object ReadJson(JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer) =>
      IPAddress.Parse((String) reader.Value);
  }
}
