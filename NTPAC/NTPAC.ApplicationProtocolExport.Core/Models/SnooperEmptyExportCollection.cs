namespace NTPAC.ApplicationProtocolExport.Core.Models
{
  public class SnooperEmptyExportCollection : SnooperExportCollection
  {
    public static readonly SnooperEmptyExportCollection Instance = new SnooperEmptyExportCollection();
    
    private SnooperEmptyExportCollection() { }
  }
}
