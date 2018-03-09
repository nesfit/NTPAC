using System;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Models;
using NTPAC.Messages.L7ConversationHandler;
using NTPAC.Persistence.Interfaces;

namespace NTPAC.ConversationTracking.Actors
{
  public class L7ConversationStorageActor : ReceiveActor
  {
    private const Int32 ConcurrentRepositoryWrites = 512;
    
    private readonly IActorRef _contractor;
    
    private readonly ICaptureFacade _captureFacade;
    private readonly IL7ConversationFacade _conversationL7Facade;
    private readonly IPcapFacade _pcapFacade;
    
    private readonly SemaphoreSlim _throttlingSem = new SemaphoreSlim(ConcurrentRepositoryWrites);
    private Capture _capture;
    private Boolean _isCompleting;
    private Int32 _l7ConversationInsertsInProgress;
    
    private readonly ILoggingAdapter _logger = Context.GetLogger();
    
    public L7ConversationStorageActor(IActorRef contractor,
                                      ICaptureInfo captureInfo,
                                      ICaptureFacade captureFacade,
                                      IL7ConversationFacade conversationL7Facade,
                                      IPcapFacade pcapFacade)
    {
      this._contractor           = contractor;
      this._captureFacade        = captureFacade;
      this._conversationL7Facade = conversationL7Facade;
      this._pcapFacade = pcapFacade;

      this.Become(this.StoringCaptureInfo);

      this.Self.Tell(captureInfo);
    }

    public static Props Props(IActorRef contractor,
                              ICaptureInfo captureInfo,                        
                              ICaptureFacade captureFacade,
                              IL7ConversationFacade conversationL7Facade,
                              IPcapFacade pcapFacade) =>
      Akka.Actor.Props.Create<L7ConversationStorageActor>(contractor, captureInfo, captureFacade, conversationL7Facade, pcapFacade);

    private async Task Complete()
    {
      await this._captureFacade.UpdateAsync(this._capture);

      this._contractor.Tell(L7ConversationHandlerCompleted.Instance);
      Context.Stop(this.Self);
    }

    private Capture CreateCaptureEntity(CaptureInfo captureInfo)
    {
      var address = Context.Self.Path.Address;
      var capture = new Capture(captureInfo, address.HasGlobalScope ? address.ToString() : null);
      this._captureFacade.InsertAsync(capture).Wait();
      return capture;
    }

    private void OnL7Conversation(L7Conversation l7Conversation)
    {
      this._logger.Debug($"Storing L7 Conversation: {l7Conversation}");
      l7Conversation.CaptureId = this._capture.Id;

      this._capture.UpdateForL7Conversation(l7Conversation);
      this._l7ConversationInsertsInProgress++;

      this.StoreL7ConversationAsync(l7Conversation).PipeTo(this.Self);
    }

    private async Task<L7ConversationStored> StoreL7ConversationAsync(L7Conversation l7Conversation)
    {
      await this._throttlingSem.WaitAsync().ConfigureAwait(false);
      try
      { 
        var t1 = this._conversationL7Facade.InsertAsync(l7Conversation);
        var t2 = this._pcapFacade.StoreL7ConversationAsync(l7Conversation);
        await Task.WhenAll(t1, t2).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        this._logger.Error(e, $"StoreL7ConversationAsync error: {e.Message}");
        throw;
      }
      finally
      {
        this._throttlingSem.Release();
      }

      return L7ConversationStored.Instance;
    }

    private async Task OnL7ConversationStorageCompleteAsync()
    {
      if (this._l7ConversationInsertsInProgress > 0)
      {
        this._isCompleting = true;
      }
      else
      {
        await this.Complete();
      }
    }

    private async Task OnL7ConversationStoredAsync()
    {
      if (this._l7ConversationInsertsInProgress <= 0)
      {
        throw new Exception("L7ConversationStoragesInProgress will decrease bellow zero");
      }

      this._l7ConversationInsertsInProgress--;

      if (this._l7ConversationInsertsInProgress == 0 && this._isCompleting)
      {
        await this.Complete();
      }
    }

    private void ProcessingL7Conversations()
    {
      this.Receive<L7Conversation>(l7Conversation => this.OnL7Conversation(l7Conversation));
      this.ReceiveAsync<L7ConversationStored>(async _ => await this.OnL7ConversationStoredAsync());
      this.ReceiveAsync<L7ConversationHandlerComplete>(async _ => await this.OnL7ConversationStorageCompleteAsync());
    }

    private void StoringCaptureInfo() =>
      this.Receive<CaptureInfo>(captureInfo =>
      {
        this._capture = this.CreateCaptureEntity(captureInfo);
        this.Become(this.ProcessingL7Conversations);
      });
    
    
    private class L7ConversationStored
    {
      public static readonly L7ConversationStored Instance = new L7ConversationStored();
    }
  }
}
