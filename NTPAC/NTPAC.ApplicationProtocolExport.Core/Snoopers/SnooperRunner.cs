using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NTPAC.ApplicationProtocolExport.Core.ApplicationProtocolClassifiers.PortBasedClassifier;
using NTPAC.ApplicationProtocolExport.Core.Models;
using NTPAC.ConversationTracking.Interfaces;

namespace NTPAC.ApplicationProtocolExport.Core.Snoopers
{
  public class SnooperRunner
  {
    private readonly PortBasedClassifier _portBasedClassifier = new PortBasedClassifier();

    private readonly Dictionary<Type, IEnumerable<String>> _snooperTypesWithApplicationTags =
      new Dictionary<Type, IEnumerable<String>>();

    public void RegisterSnooper<TSnooper>() where TSnooper:SnooperBase
    {
      var snooperType = typeof(TSnooper);
      if (this._snooperTypesWithApplicationTags.ContainsKey(snooperType))
      {
        throw new ArgumentException($"Snooper {snooperType} already registered");
      }
      
      var snooper =  (SnooperBase) Activator.CreateInstance(snooperType);
      var applicationProtocolTags = snooper.ApplicationProtocolTags;
      this._snooperTypesWithApplicationTags.Add(snooperType, applicationProtocolTags);
    }

    public IEnumerable<SnooperExportCollection> Run(IEnumerable<IL7Conversation> l7Conversations) => l7Conversations.Select(this.Run);

    public SnooperExportCollection Run(IL7Conversation conversation)
    {
      if (!conversation.Pdus.Any())
      {
        return SnooperEmptyExportCollection.Instance;
      }
      
      var protocolTag = this._portBasedClassifier.Classify(conversation);
      if (protocolTag == null)
      {
        // No known protocol match
        return SnooperEmptyExportCollection.Instance;
      }

      var snooper = this.CreateSnooperForApplicationProtocolTag(protocolTag);
      if (snooper == null)
      {
        return SnooperEmptyExportCollection.Instance;
      }
      
      var snooperExportCollection = snooper.ProcessConversation(conversation);
      return snooperExportCollection;
    }

    private SnooperBase CreateSnooperForApplicationProtocolTag(String tag)
    {
      foreach (var (type, tags) in this._snooperTypesWithApplicationTags)
      {
        if (tags.Contains(tag))
        {
          return (SnooperBase) Activator.CreateInstance(type);
        }
      }

      return null;
    }
  }
}
