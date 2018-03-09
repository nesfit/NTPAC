using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using NTPAC.Persistence.DTO.ConversationTracking;
using NTPAC.Persistence.Entities;

namespace NTPAC.Persistence.Generic.Facades.Converters
{
  internal sealed class IPEndPointEntityConverter : TypeConverter
  {
    public override Boolean CanConvertFrom(ITypeDescriptorContext context, Type dstType) => dstType == typeof(IPEndPointEntity);
    public override Boolean CanConvertTo(ITypeDescriptorContext context, Type dstType) =>
      dstType == typeof(IPEndPointDTO) || dstType == typeof(IPEndPoint);

    public override Object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, Object obj, Type dstType)
    {
      var ipEndPointEntity = (IPEndPointEntity) obj;
      
      if (dstType == typeof(IPEndPointDTO)) {
        return new IPEndPointDTO
             {
               Port          = ipEndPointEntity.Port,
               Address       = ipEndPointEntity.Address.ToString(),
               AddressFamily = ipEndPointEntity.AddressFamily
             };
      }

      if (dstType == typeof(IPEndPoint))
      {
        return new IPEndPoint(ipEndPointEntity.Address, ipEndPointEntity.Port);
      }
      
      throw  new ArgumentException("Invalid dstType");
    }
  }
}
