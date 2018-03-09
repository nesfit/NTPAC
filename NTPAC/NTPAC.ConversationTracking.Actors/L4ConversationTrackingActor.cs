using System;
using System.Collections.Generic;
using System.Net;
using Akka.Actor;
using Akka.Event;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Models;
using NTPAC.Messages.L4ConversationTracking;
using NTPAC.Reassembling;
using NTPAC.Reassembling.Exceptions;

namespace NTPAC.ConversationTracking.Actors
{
  public class L4ConversationTrackingActor : ReceiveActor
  {
    private readonly IActorRef _contractor;
    private readonly IL4ConversationKey _l4Key;
    private readonly List<IActorRef> _l7ConversationHandlerActors;
    private readonly L7ConversationTrackerBase _l7ConversationTracker;
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    public L4ConversationTrackingActor(IL4ConversationKey l4Key,
                                       IActorRef contractor,
                                       IPEndPoint sourceEndPoint,
                                       IPEndPoint destinationEndPoint,
                                       List<IActorRef> l7ConversationHandlerActors)
    {
      this._l4Key                      = l4Key;
      this._contractor                 = contractor;
      this._l7ConversationHandlerActors = l7ConversationHandlerActors;
      this._l7ConversationTracker =
        L7ConversationTrackerFactory.Create(sourceEndPoint, destinationEndPoint, l4Key.GetProtocolType);
      
      this.Become(this.AnalysisBehavior);
    }

    public static Props Props(IL4ConversationKey l4Key,
                              IActorRef contractor,
                              IPEndPoint sourceEndPoint,
                              IPEndPoint destinationEndPoint,
                              List<IActorRef> l7ConversationHandlerActors) =>
      Akka.Actor.Props.Create<L4ConversationTrackingActor>(l4Key, contractor, sourceEndPoint, destinationEndPoint,
                                                           l7ConversationHandlerActors);

    private void AnalysisBehavior()
    {
      this.Receive<Frame>(frame => this.OnFrame(frame));
      this.Receive<L4ConversationTrackingComplete>(complete => this.OnL4ConversationTrackingComplete(complete));
    }

    private void HandleL7Conversation(L7Conversation l7Conversation)
    {
      if (l7Conversation == null)
      {
        return;
      }

      //this._logger.Debug($"L4C HandleNewL7Conversation: {l7Conversation}");

      // Performance optimization
      // ReSharper disable once ForCanBeConvertedToForeach
      for (var i = 0; i < this._l7ConversationHandlerActors.Count; i++)
      {
        var l7ConversationHandlerActor = this._l7ConversationHandlerActors[i];
        l7ConversationHandlerActor.Tell(l7Conversation);
      }
    }

    private void HandleL7Conversations(IEnumerable<L7Conversation> l7Conversations)
    {
      if (l7Conversations == null)
      {
        return;
      }

      foreach (var l7Conversation in l7Conversations)
      {
        this.HandleL7Conversation(l7Conversation);
      }
    }

    private void OnFrame(Frame frame)
    {
      //this._logger.Debug("L4C OnFrame");
      
      L7Conversation l7Conversation;
      try
      {
        l7Conversation = this._l7ConversationTracker.ProcessFrame(frame);
      }
      catch (ReassemblingException e)
      {
        this._logger.Error(e, $"Processing of a frame {e.Frame} caused exception in reassembler: {e.Message}");
        throw;
      }

      this.HandleL7Conversation(l7Conversation);
    }

    private void OnL4ConversationTrackingComplete(L4ConversationTrackingComplete complete)
    {    
      this._logger.Debug("L4C OnFinalizeProcessing");

      IEnumerable<L7Conversation> l7Conversations;
      try
      {
        l7Conversations = this._l7ConversationTracker.Complete();
      }
      catch (ReassemblingException e)
      {
        this._logger.Error(e, $"Frame {e.Frame} caused exception: {e.Message}");
        throw;
      }

      this.HandleL7Conversations(l7Conversations);

      this._contractor.Tell(new L4ConversationTrackingCompleted(this._l4Key, complete.CompletedByInactivity));
      Context.Stop(this.Self);
    }
  }
}
