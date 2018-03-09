using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using NTPAC.AkkaSupport;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Factories;
using NTPAC.ConversationTracking.Models;
using NTPAC.Messages.CaptureTracking;
using NTPAC.Messages.L3ConversationTracking;
using NTPAC.Messages.L7ConversationHandler;
using NTPAC.Reassembling.IP;
using PacketDotNet;

namespace NTPAC.ConversationTracking.Actors
{
  public class CaptureTrackingActor : ReceiveActor
  {
    private readonly ICaptureInfo _captureInfo;
    private readonly IActorRef _contractor;
    private readonly Ipv4DefragmentationEngine _ipv4DefragmentationEngine;

    private readonly IL3ConversationTrackingActorFactory _l3ConversationTrackingActorFactory;

    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private readonly Dictionary<IL3ConversationKey, IActorRef> _l3Conversations = new Dictionary<IL3ConversationKey, IActorRef>();
    private readonly List<IActorRef> _l7ConversationHandlerActors;

    private TaskCompletionSource<Object> _l3ConversationsCompletedTCS;
    private TaskCompletionSource<Object> _l7ConversationHandlersCompletedTCS;
    
    private CaptureTrackingComplete _captureTrackingCompleteRequest;

    private Int64 _lastInactivityCheckTimestampTicks;
    private readonly Int64 _inactivityCheckIntervalTicks = TimeSpan.FromMinutes(64).Ticks;
    private readonly Int64 _inactivityTimeoutTicks = TimeSpan.FromMinutes(60).Ticks;
    
    public CaptureTrackingActor(ICaptureInfo captureInfo,
                                IActorRef contractor,
                                IL3ConversationTrackingActorFactory l3ConversationTrackingActorFactory,
                                IL7ConversationStorageActorFactory l7ConversationStorageActorFactory,
                                IApplicationProtocolExportActorFactory applicationProtocolExportActorFactory)
    {
      this._logger.Debug($"Started for {captureInfo.Uri.AbsoluteUri}");
      
      this._captureInfo                        = captureInfo;
      this._contractor                         = contractor;
      this._l3ConversationTrackingActorFactory = l3ConversationTrackingActorFactory;
      this._ipv4DefragmentationEngine =
        new Ipv4DefragmentationEngine(new AkkaLoggingAdapter<Ipv4DefragmentationEngine>(this._logger));

      this._l7ConversationHandlerActors = this.CreateL7ConversationHandlerActors(l7ConversationStorageActorFactory, applicationProtocolExportActorFactory);

      this.Become(this.AnalysisBehavior);
    }

    public static Props Props(ICaptureInfo info,
                              IActorRef contractor,
                              IL3ConversationTrackingActorFactory l3ConversationTrackingActorFactory,
                              IL7ConversationStorageActorFactory l7ConversationStorageActorFactory,
                              IApplicationProtocolExportActorFactory applicationProtocolExportActorFactory) =>
      Akka.Actor.Props.Create<CaptureTrackingActor>(info, contractor, l3ConversationTrackingActorFactory,
                                                             l7ConversationStorageActorFactory, applicationProtocolExportActorFactory);
   
    private List<IActorRef> CreateL7ConversationHandlerActors(IL7ConversationStorageActorFactory l7ConversationStorageActorFactory,
                                                              IApplicationProtocolExportActorFactory applicationProtocolExportActorFactory)
    {
      var l7ConversationFilter = new Predicate<IL7Conversation>(
        l7c => l7c.ProtocolType == IPProtocolType.TCP && l7c.DestinationEndPoint.Port == 80 ||
               l7c.ProtocolType == IPProtocolType.UDP && l7c.DestinationEndPoint.Port == 53);
      
      // Register L7 conversation handlers
      var l7ConversationHandlerActors = new List<IActorRef>
                                         {
                                           l7ConversationStorageActorFactory.Create(Context, this.Self, this._captureInfo),
                                           applicationProtocolExportActorFactory.Create(Context, this.Self, l7ConversationFilter)
                                         };
      
      foreach (var l7conversationHandlerActor in l7ConversationHandlerActors)
      {
        Context.Watch(l7conversationHandlerActor);
      }
      return l7ConversationHandlerActors;
    }

    #region Completing Behaviour
    private void CompletingBehaviour()
    {
      this.Receive<L3ConversationTrackingCompleted>(completed => this.OnL3ConversationTrackingCompleted(completed));
      this.Receive<L7ConversationHandlerCompleted>(completed => this.OnL7ConversationHandlerCompleted(completed));
     
      this.Receive<CaptureTrackingComplete>(_ =>
      {
        this.CompleteL3ConversationTrackersAsync().PipeTo(this.Self);
      });
      this.Receive<L3ConversationTrackersCompleted>(_ =>
      {
        this.CompleteL7ConversationHandlersAsync().PipeTo(this.Self);
      });
      this.Receive<L7ConversationHandlersCompleted>(_ =>
      {
//        this._logger.Info("Sending CaptureTrackingCompleted");
        var captureTrackingCompleted = new CaptureTrackingCompleted {MessageId = this._captureTrackingCompleteRequest.MessageId};
        this._contractor.Tell(captureTrackingCompleted);
        Context.Stop(this.Self);
      });
    }
    
    private void OnL7ConversationHandlerCompleted(L7ConversationHandlerCompleted completed)
    {
      if (!this._l7ConversationHandlerActors.Remove(this.Sender))
      {
        this._logger.Error($"{this.Sender} not removed from _l7ConversationHandlerActors");
        return;
      }
      
      Context.Unwatch(this.Sender);
      
      if (!this._l7ConversationHandlerActors.Any())
      {
        this._l7ConversationHandlersCompletedTCS.SetResult(null);
      }
    }
    
    private async Task<L3ConversationTrackersCompleted> CompleteL3ConversationTrackersAsync()
    {
      if (!this._l3Conversations.Any())
      {
        this._logger.Info("No L3ConversationTracker to send L3ConversationTrackingComplete to");
        return L3ConversationTrackersCompleted.Instance;
      }
      
//      this._logger.Info($"Sending L3ConversationTrackingComplete {this._l3Conversations.Count}");
      
      foreach (var l3ConversationActor in this._l3Conversations.Values)
      {
        l3ConversationActor.Tell(L3ConversationTrackingComplete.Instance);
      }
      
      await this._l3ConversationsCompletedTCS.Task.ConfigureAwait(false);
//      this._logger.Info("All L3ConversationTrackingCompleted gathered");
      return L3ConversationTrackersCompleted.Instance;
    }

    private async Task<L7ConversationHandlersCompleted> CompleteL7ConversationHandlersAsync()
    {
      if (!this._l7ConversationHandlerActors.Any())
      {
//        this._logger.Info("No L7ConversationHandler to send L7ConversationHandlerComplete to");
        return L7ConversationHandlersCompleted.Instance;
      }
      
//      this._logger.Info("Sending L7ConversationHandlerComplete");
      
      foreach (var l7ConversationHandler in this._l7ConversationHandlerActors)
      {
        l7ConversationHandler.Tell(L7ConversationHandlerComplete.Instance);
      }
      await this._l7ConversationHandlersCompletedTCS.Task.ConfigureAwait(false);
//      this._logger.Info("All L7ConversationHandlerCompleted gathered");
      return L7ConversationHandlersCompleted.Instance;
    }

    private class L3ConversationTrackersCompleted
    {
      public static readonly L3ConversationTrackersCompleted Instance = new L3ConversationTrackersCompleted();
    }
    
    private class L7ConversationHandlersCompleted
    {
      public static readonly L7ConversationHandlersCompleted Instance = new L7ConversationHandlersCompleted();
    }
    #endregion
    
    
    #region Anylysis Behaviour
    private void AnalysisBehavior()
    {
      this.Receive<Frame>(frame => this.OnFrame(frame));
      this.Receive<CaptureTrackingComplete>(complete => this.OnCaptureTrackingComplete(complete));
      this.Receive<L3ConversationTrackingCompleted>(completed => this.OnL3ConversationTrackingCompleted(completed));
    }
    
    private void OnCaptureTrackingComplete(CaptureTrackingComplete complete)
    {
      this._logger.Debug("Received CaptureTrackingComplete request");
      this._captureTrackingCompleteRequest = complete;
      this._l3ConversationsCompletedTCS        = new TaskCompletionSource<Object>();
      this._l7ConversationHandlersCompletedTCS = new TaskCompletionSource<Object>();
      this.Become(this.CompletingBehaviour);
      Self.Forward(complete);
    }
    
    private void OnFrame(Frame frame)
    {
      if (frame.IsIpv4Fragmented)
      {
        if (!this.TryDefragmentFrame(ref frame))
        {
          return;
        }
      }

      this.TrackL3ConversationForFrame(frame);
    }
    
    private void OnL3ConversationTrackingCompleted(L3ConversationTrackingCompleted completed)
    {
      if (!this._l3Conversations.Remove(completed.L3ConversationKey))
      {
        this._logger.Error($"{this.Sender} not removed from _l3Conversations");
        return;
      }
      if (!this._l3Conversations.Any())
      {
        _l3ConversationsCompletedTCS?.SetResult(null);
      }
    }

    private void TrackL3ConversationForFrame(Frame frame)
    {
      var l3Key = frame.L3ConversationKey;
      if (!this._l3Conversations.TryGetValue(l3Key, out var l3ConversationActor))
      {
        l3ConversationActor = this.CreateL3ConversationActor(l3Key);
      }

      l3ConversationActor.Tell(frame);
      
      this.CheckL3ConversationsInactivity(frame);
    }

    private IActorRef CreateL3ConversationActor(IL3ConversationKey l3Key)
    {
      this._logger.Debug($"Creating new L3C actor: {l3Key}");
      var l3ConversationActor =
        this._l3ConversationTrackingActorFactory.Create(Context, l3Key, this.Self, this._l7ConversationHandlerActors);
      this._l3Conversations.Add(l3Key, l3ConversationActor);
      return l3ConversationActor;
    }
    
    private Boolean TryDefragmentFrame(ref Frame frame)
    {
      var (isDefragmentationSuccessful, defragmentedIpv4Packet, fragments) =
        this._ipv4DefragmentationEngine.TryDefragmentFragments(frame);
      if (!isDefragmentationSuccessful)
      {
        return false;
      }

      frame = FrameFactory.CreateFromDefragmentedIpPacket(defragmentedIpv4Packet, fragments);
      return true;
    }

    private void CheckL3ConversationsInactivity(Frame frame)
    {
      var currentTimestampTicks = frame.TimestampTicks;

      if (this._lastInactivityCheckTimestampTicks == 0)
      {
        // Set for first frame
        this._lastInactivityCheckTimestampTicks = currentTimestampTicks;
        return;
      }
      if (currentTimestampTicks <= this._lastInactivityCheckTimestampTicks + this._inactivityCheckIntervalTicks)
      {
        return;
      }
      
      this._lastInactivityCheckTimestampTicks = currentTimestampTicks;
      
      var inactivityCheckRequest = new L3ConversationTrackingInactivityCheck(currentTimestampTicks, this._inactivityTimeoutTicks);
      foreach (var l3ConversationActor in this._l3Conversations.Values)
      {
        l3ConversationActor.Tell(inactivityCheckRequest);
      }
    }
    #endregion 
  }
}
