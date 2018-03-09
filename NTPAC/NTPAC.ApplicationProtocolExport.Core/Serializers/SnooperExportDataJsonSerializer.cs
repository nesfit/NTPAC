using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.Serializers.Converters;

namespace NTPAC.ApplicationProtocolExport.Core.Serializers
{
  public static class SnooperExportDataJsonSerializer
  {
    public static String Serialize(SnooperExportBase snooperExport) =>
      JsonConvert.SerializeObject(snooperExport, Settings);

    private static JsonSerializerSettings CreateSettings()
    {     
      var settings = new JsonSerializerSettings();
      settings.Converters.Add(new IPAddressConverter());
      settings.ContractResolver = new IgnoringSnooperExportBasePropertiesContractResolver();
      return settings;
    }

    private static JsonSerializerSettings Settings => CreateSettings();

    private class IgnoringSnooperExportBasePropertiesContractResolver : DefaultContractResolver
    {
      protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
      {
        var property = base.CreateProperty(member, memberSerialization);
        property.Ignored = member.ReflectedType == typeof(SnooperExportBase) ||
                           Attribute.IsDefined(member, typeof(JsonIgnoreAttribute));
        return property;
      }
    }
  }
}
