using System.IO;
using System.Threading.Tasks;
using NTPAC.ConversationTracking.Models;
using NTPAC.Persistence.Interfaces;
using SharpPcap.LibPcap;

namespace NTPAC.Persistence.Pcap.Facade
{
  public class PcapFacade : IPcapFacade
  {
    private readonly IPcapFacadeConfiguration _configuration;

    public PcapFacade(IPcapFacadeConfiguration configuration)
    {
      this._configuration = configuration;

      Directory.CreateDirectory(this._configuration.BaseDirectory);
    }

    public Task StoreL7ConversationAsync(L7Conversation l7Conversation) => 
      Task.Run(() => this.StoreL7Conversation(l7Conversation));
    
    public void StoreL7Conversation(L7Conversation l7Conversation)
    {
      var pcapFilename = $"{l7Conversation.Id}.pcapng";
      var pcapPath = Path.Combine(this._configuration.BaseDirectory, pcapFilename);

      CaptureFileWriterDevice pcapWriterDevice = null;
      try
      {
        pcapWriterDevice = new CaptureFileWriterDevice(pcapPath);
        var rawCaptures = l7Conversation.ReconstructRawCaptures();
        foreach (var rawCapture in rawCaptures)
        {
          pcapWriterDevice.Write(rawCapture);
        }
      }
      finally
      {
        pcapWriterDevice?.Close();
      }
    }
  }
}
