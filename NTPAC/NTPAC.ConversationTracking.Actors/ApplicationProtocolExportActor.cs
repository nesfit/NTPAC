using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.Util;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ApplicationProtocolExport.Core.Serializers;
using NTPAC.ApplicationProtocolExport.Core.Snoopers;
using NTPAC.ConversationTracking.Interfaces;
using NTPAC.ConversationTracking.Models;
using NTPAC.Messages.L7ConversationHandler;
using NTPAC.Persistence.Interfaces;
using SnooperDNS;
using SnooperHTTP;

namespace NTPAC.ConversationTracking.Actors
{
  public class ApplicationProtocolExportActor : ReceiveActor
  {
    private readonly IActorRef _contractor;
    
    private ISourceQueueWithComplete<L7Conversation> _l7ConversationStreamSourceQueue;
    private Task _processingStreamCompletitionTask;
    
    private readonly ILoggingAdapter _logger = Context.GetLogger();
    
    public ApplicationProtocolExportActor(IActorRef contractor, Predicate<L7Conversation> l7ConversationFilter, ISnooperExportFacade snooperExportFacade)
    {
      this._contractor = contractor;
      
      this.SetupAndRunProcessingPipeline(l7ConversationFilter, 4096, snooperExportFacade);
      
      this.Become(this.ProcessingBehaviour);
    }
    
    public static Props Props(IActorRef contractor, Predicate<IL7Conversation> l7ConversationFilter, ISnooperExportFacade snooperExportFacade) =>
      Akka.Actor.Props.Create<ApplicationProtocolExportActor>(contractor, l7ConversationFilter, snooperExportFacade);
    
    private SnooperRunner CreateAndInitializeSnooperRunner()
    {
      var snooperRunner = new SnooperRunner();
      
      snooperRunner.RegisterSnooper<SnooperDns>();
      snooperRunner.RegisterSnooper<SnooperHttp>();

      return snooperRunner;
    }

    private void SetupAndRunProcessingPipeline(Predicate<L7Conversation> l7ConversationFilter, Int32 sourceQueueSize, ISnooperExportFacade snooperExportFacade)
    {
      var snooperRunner = this.CreateAndInitializeSnooperRunner();
      
      var storageAndLogSink = Sink.ForEachParallel<SnooperExportCollection>(
          1024, async snooperExportCollection =>
          {
#if DEBUG
            this.LogSnooperExportCollectionExceptions(snooperExportCollection);
#endif            
            await StoreSnooperExportCollectionAsync(snooperExportFacade, snooperExportCollection).ConfigureAwait(false);
          });

      var l7ConversationStreamSource = Source.Queue<L7Conversation>(sourceQueueSize, OverflowStrategy.Backpressure)
                                             .Where(l7ConversationFilter).SelectAsyncUnordered(
                                               Environment.ProcessorCount,
                                               conversation => this.ProcessConversationAsync(conversation, snooperRunner))
                                             .WhereNot(collection => collection is SnooperEmptyExportCollection);

      (this._l7ConversationStreamSourceQueue, this._processingStreamCompletitionTask) = l7ConversationStreamSource
                                              .ToMaterialized(storageAndLogSink, Keep.Both)
                                              .Run(Context.System.Materializer());
    }

    private Task<SnooperExportCollection> ProcessConversationAsync(L7Conversation conversation, SnooperRunner snooperRunner)
    {
      return Task.Run(() => snooperRunner.Run(conversation));
    }
    
    private void LogSnooperExportCollectionExceptions(SnooperExportCollection snooperExportCollection)
    {
      foreach (var export in snooperExportCollection.Exports.Where(export => export.ParsingFailed))
      {
        this._logger.Error(export.ParsingError, $"Snooper {snooperExportCollection.SnooperId} caused exception while parsing L7 conversation {export.Conversation}. Last PDU {export.Pdu}");
      }
    }
    
    private async Task StoreSnooperExportCollectionAsync(ISnooperExportFacade snooperExportFacade, SnooperExportCollection snooperExportCollection)
    {  
      try
      {
        await snooperExportFacade.InsertAsync(snooperExportCollection).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        this._logger.Error(e, "SnooperExportCollection storage error");
        throw;
      }
    }

    private void ProcessingBehaviour()
    {
      this.Receive<L7Conversation>(l7Conversation => this.OnL7Conversation(l7Conversation));
      this.Receive<L7ConversationHandlerComplete>(complete => this.OnL7ConversationHandlerComplete(complete));
      this.Receive<IQueueOfferResult>(msg => this.OnQueueOfferResult(msg));
      this.Receive<ProcessingPipelineCompleted>(_ => this.OnProcessingPipelineCompleted());
    }
    
    private void OnL7Conversation(L7Conversation l7Conversation)
    {
      this._l7ConversationStreamSourceQueue.OfferAsync(l7Conversation).PipeTo(this.Self);
    }
    
    private void OnQueueOfferResult(IQueueOfferResult queueOfferResult)
    {
      switch (queueOfferResult)
      {
//        case QueueOfferResult.Enqueued _:
//          Console.WriteLine("Enqueued");
//          break;
        case QueueOfferResult.Dropped a:
          this._logger.Error("Queue offer: Dropped");
          break;
        case QueueOfferResult.Failure _:
          this._logger.Error("Queue offer: Failure");
          break;
        case QueueOfferResult.QueueClosed _:
          this._logger.Error("Queue offer: QueueClosed");
          break;
      }
    }

    private void OnL7ConversationHandlerComplete(L7ConversationHandlerComplete complete)
    {     
      this._l7ConversationStreamSourceQueue.Complete();
      Task.Run(async () =>
      {
        try
        {
          await this._l7ConversationStreamSourceQueue.WatchCompletionAsync().ConfigureAwait(false);
          await this._processingStreamCompletitionTask.ConfigureAwait(false);
        }
        catch (Exception e)
        {
          this._logger.Error(e,"Failed to complete APE stream");
        }
        return ProcessingPipelineCompleted.Instance;
        }).PipeTo(this.Self);
    }

    private void OnProcessingPipelineCompleted()
    {     
      this._contractor.Tell(L7ConversationHandlerCompleted.Instance);
      Context.Stop(this.Self);
    }
    
    private class ProcessingPipelineCompleted
    {
      public static readonly ProcessingPipelineCompleted Instance = new ProcessingPipelineCompleted();
    }
  }
}
