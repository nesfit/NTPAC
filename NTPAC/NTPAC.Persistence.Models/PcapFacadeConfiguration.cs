using System;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.Persistence.Models
{
  public class PcapFacadeConfiguration : IPcapFacadeConfiguration
  {
    public String BaseDirectory { get; set; }
  }
}
