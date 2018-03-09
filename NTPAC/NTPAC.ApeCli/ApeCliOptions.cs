using System;
using CommandLine;
using NTPAC.Common;

namespace NTPAC.ApeCli
{
  public class ApeCliOptions
  {
    [Option("pcap-provider", HelpText = "Use PCAP L7 conversation provider with specified PCAP file")]
    public RelativeFileUri PcapProviderUri { get; set; }  
    
//    [Option("cassandra-provider", HelpText = "Use Cassandra DB L7 conversation provider")]
//    public Boolean CassandraProvider { get; set; }
    
    [Option("cassandra-contactpoint", Default = "localhost", HelpText = "Cassandra contact node's hostname")]
    public String CassandraContactPoint { get; set; }

    [Option("cassandra-keyspace", Default = "ntpac", HelpText = "Cassandra keyspace")]
    public String CassandraKeyspace { get; set; }
    
    [Option("l7c-filter")]
    public String L7ConversationFilter { get; set; }
    
//    [Option("devnull-repository", Default = false, HelpText = "Don't save any persisted objects - dispose immediately")]
//    public Boolean DevnullRepository { get; set; }
//
//    [Option("inmemory-repository", Default = false, HelpText = "Don't save any persisted objects")]
//    public Boolean InMemoryRepository { get; set; }
//    
//    [Option("cassandra-repository", Default = false, HelpText = "Use Cassandra repository")]
//    public Boolean CassandraRepository { get; set; }
    
    [Option("print-snooper-exports", Default = false, HelpText = "Just prints snoopers' exports instead of storing them in a repository")]
    public Boolean PrintSnooperExports { get; set; }
  }
}
