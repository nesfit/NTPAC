using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.Serializers.Converters;

namespace NTPAC.ApplicationProtocolExport.Core.Serializers
{
  public static class SnooperExportCollectionsPrettyJsonSerializer
  {
    public static String Serialize(IEnumerable<SnooperExportCollection> snoopersExports) =>
      JsonConvert.SerializeObject(snoopersExports, Settings);

    public static String Serialize(SnooperExportCollection snooperExport) =>
      JsonConvert.SerializeObject(snooperExport, Settings);

    private static JsonSerializerSettings CreateSettings()
    {     
      var settings = new JsonSerializerSettings();
      settings.Converters.Add(new IPAddressConverter());
      settings.Formatting = Formatting.Indented;
      return settings;
    }

    private static JsonSerializerSettings Settings => CreateSettings();
  }
}
