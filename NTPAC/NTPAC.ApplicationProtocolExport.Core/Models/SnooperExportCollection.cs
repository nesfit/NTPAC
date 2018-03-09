using System;
using System.Collections.Generic;
using NTPAC.ApplicationProtocolExport.Core.Snoopers;

namespace NTPAC.ApplicationProtocolExport.Core.Models
{
  public class SnooperExportCollection
  {
    public readonly String SnooperId;
    public readonly IReadOnlyCollection<SnooperExportBase> Exports;
    
    protected SnooperExportCollection() { }
    
    private SnooperExportCollection(SnooperBase snooper) => this.SnooperId = snooper.SnooperId;

    public SnooperExportCollection(SnooperBase snooper,
                                   IReadOnlyCollection<SnooperExportBase> exportedObjects) : this(snooper)
    {
      this.Exports = exportedObjects;
    }     
    public override String ToString() => $"{this.SnooperId}: {this.Exports.Count} exports";
  }
}
