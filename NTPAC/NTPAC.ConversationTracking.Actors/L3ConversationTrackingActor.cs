using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using NTPAC.AkkaSupport.Collections;
using NTPAC.Common.Extensions;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Models;
using NTPAC.Messages.L3ConversationTracking;
using NTPAC.Messages.L4ConversationTracking;
using NTPAC.Reassembling;
using Debug = System.Diagnostics.Debug;

namespace NTPAC.ConversationTracking.Actors
{
  public class L3ConversationTrackingActor : ReceiveActor
  {
    private readonly IActorRef _contractor;
    private readonly IL3ConversationKey _l3Key;

    private readonly Dictionary<IL4ConversationKey, IActorRef> _l4Conversations = new Dictionary<IL4ConversationKey, IActorRef>();
    
    private Int64 _lastActivityTimestampTicks;

    private readonly AgingActorBuffer _l4ConversationsAgingBuffer = new AgingActorBuffer(L7ConversationTrackerBase.SessionTimeoutTicks);
    private readonly IL4ConversationTrackingActorFactory _l4ConversationTrackingActorFactory;
    
    private TaskCompletionSource<Object> _l4ConversationsCompletedTCS;
    
    private readonly List<IActorRef> _l7ConversationHandlerActors;

    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public L3ConversationTrackingActor(IL3ConversationKey l3Key,
                                       IActorRef contractor,
                                       List<IActorRef> l7ConversationHandlerActors,
                                       IL4ConversationTrackingActorFactory l4ConversationTrackingActorFactory)
    {
      this._l3Key                              = l3Key;
      this._contractor                         = contractor;
      this._l7ConversationHandlerActors        = l7ConversationHandlerActors;
      this._l4ConversationTrackingActorFactory = l4ConversationTrackingActorFactory;

      this.Become(this.ProcessingBehavior);
    }

    public static Props Props(IL3ConversationKey l3Key,
                              IActorRef contractor,
                              List<IActorRef> l7ConversationHandlerActors,
                              IL4ConversationTrackingActorFactory l4ConversationTrackingActorFactory) =>
      Akka.Actor.Props.Create<L3ConversationTrackingActor>(l3Key, contractor, l7ConversationHandlerActors,
                                              l4ConversationTrackingActorFactory);

    private void ProcessingBehavior()
    {
      this.Receive<Frame>(frame => this.OnFrame(frame));
      this.Receive<L4ConversationTrackingCompleted>(completed => this.OnL4ConversationTrackingCompleted(completed));
      this.Receive<L3ConversationTrackingComplete>(_ => this.OnL3ConversationTrackingComplete());
      this.Receive<L4ConversationTrackersCompleted>(_ => this.StopL3ConversationTracking());
      this.Receive<L3ConversationTrackingInactivityCheck>(inactivityCheck =>
                                                            this.OnL3ConversationTrackingInactivityCheck(inactivityCheck));
    }
    
    private void OnFrame(Frame frame)
    {
      //this._logger.Debug("L3C OnProcessPacket");

      var l4Key = frame.L4ConversationKey;
      if (!this._l4Conversations.TryGetValue(l4Key, out var l4ConversationActor))
      {
        l4ConversationActor = this._l4ConversationTrackingActorFactory.Create(Context, l4Key, this.Self, frame.SourceEndPoint,
                                                                              frame.DestinationEndPoint,
                                                                              this._l7ConversationHandlerActors,
                                                                              frame.TimestampTicks);
        this._l4Conversations.Add(l4Key, l4ConversationActor);
      }

      l4ConversationActor.Forward(frame);

      this.UpdateLastActivityTimestamp(frame);
      this.UpdateAndCompleteInactiveL4Conversations(l4ConversationActor, frame);
    }

    private void OnL3ConversationTrackingComplete()
    {
      _l4ConversationsCompletedTCS = new TaskCompletionSource<Object>();
      this.CompleteL4ConversationTrackersAsync().PipeTo(this.Self);
    }

    private void OnL4ConversationTrackingCompleted(L4ConversationTrackingCompleted completed)
    {
      if (completed.CompletedByInactivity)
      {
        return;
      }
      if (!this._l4Conversations.Remove(completed.L4ConversationKey))
      {
        this._logger.Error($"{this.Sender} not removed from _l4Conversations");
      }
      if (!this._l4Conversations.Any())
      {
        this._l4ConversationsCompletedTCS?.SetResult(null);
      }
    }

    private void OnL3ConversationTrackingInactivityCheck(L3ConversationTrackingInactivityCheck inactivityCheck)
    {
      if (inactivityCheck.CurrentTimestampTicks <= this._lastActivityTimestampTicks + inactivityCheck.InactivityTimeoutTicks)
      {
        return;
      }

      this._logger.Debug($"Detected L3 conversation inactivity after {new TimeSpan(inactivityCheck.InactivityTimeoutTicks)}. Completing inactive L4 conversations ...");
      this.UpdateCurrentTimestampAndCompleteInactiveL4Conversations(inactivityCheck.CurrentTimestampTicks);
    }

    private async Task<L4ConversationTrackersCompleted> CompleteL4ConversationTrackersAsync()
    {
      if (!this._l4Conversations.Any())
      {
        return L4ConversationTrackersCompleted.Instance;
      }
      foreach (var l4ConversationActor in this._l4Conversations.Values)
      {
        l4ConversationActor.Tell(L4ConversationTrackingComplete.Instance);
      }
      await this._l4ConversationsCompletedTCS.Task.ConfigureAwait(false);
      return L4ConversationTrackersCompleted.Instance;
    }

    private void UpdateAndCompleteInactiveL4Conversations(IActorRef l4ConversationActor, Frame frame)
    {
      this._l4ConversationsAgingBuffer.Update(l4ConversationActor, frame.TimestampTicks);
      this.CompleteInactiveL4Conversations();
    }

    private void UpdateCurrentTimestampAndCompleteInactiveL4Conversations(Int64 currentTimestampTicks) 
    {
      this._l4ConversationsAgingBuffer.UpdateCurrentTimestamp(currentTimestampTicks);
      this.CompleteInactiveL4Conversations();
    }
    
    private void CompleteInactiveL4Conversations()
    {
      if (!this._l4ConversationsAgingBuffer.HaveInactiveActors())
      {
        return;
      }

      foreach (var inactiveL4ConversationActor in this._l4ConversationsAgingBuffer.GetAndRemoveInactiveActors())
      {
        this._logger.Debug($"Sending inactivity Fin to {inactiveL4ConversationActor}");
        inactiveL4ConversationActor.Tell(L4ConversationTrackingComplete.InactivityInstance);
        this._l4Conversations.RemoveSingleReferenceValue(inactiveL4ConversationActor);
      }
    }
    
    private void UpdateLastActivityTimestamp(Frame frame)
    {
      Debug.Assert(frame.TimestampTicks >= this._lastActivityTimestampTicks);
      this._lastActivityTimestampTicks = frame.TimestampTicks;
    }

    private void StopL3ConversationTracking()
    {
      this._contractor.Tell(new L3ConversationTrackingCompleted(this._l3Key));
      Context.Stop(this.Self);
    }
    
    private class L4ConversationTrackersCompleted
    {
      public static readonly L4ConversationTrackersCompleted Instance = new L4ConversationTrackersCompleted();
    }
  }
}
