FROM microsoft/dotnet:2.1-runtime AS base
RUN apt-get update && apt-get install -y \
	net-tools iputils-ping procps netcat nano libpcap0.8 
RUN ln -s /usr/lib/x86_64-linux-gnu/libpcap.so.0.8 /usr/lib/x86_64-linux-gnu/libwpcap.so

FROM microsoft/dotnet:2.1-sdk AS publish
ARG nuget_user
ARG nuget_pass
WORKDIR /src
COPY nuget.config ./
COPY NTPAC.AkkaSupport.Interfaces/NTPAC.AkkaSupport.Interfaces.csproj NTPAC.AkkaSupport.Interfaces/
COPY NTPAC.AkkaSupport/NTPAC.AkkaSupport.csproj NTPAC.AkkaSupport/
COPY NTPAC.ApplicationProtocolExport.Core/NTPAC.ApplicationProtocolExport.Core.csproj NTPAC.ApplicationProtocolExport.Core/
COPY NTPAC.ApplicationProtocolExport.Interfaces/NTPAC.ApplicationProtocolExport.Interfaces.csproj NTPAC.ApplicationProtocolExport.Interfaces/
COPY NTPAC.ApplicationProtocolExport.Kaitai/NTPAC.ApplicationProtocolExport.Kaitai.csproj NTPAC.ApplicationProtocolExport.Kaitai/
COPY NTPAC.Common/NTPAC.Common.csproj NTPAC.Common/
COPY NTPAC.ConversationTracking.Actors/NTPAC.ConversationTracking.Actors.csproj NTPAC.ConversationTracking.Actors/
COPY NTPAC.ConversationTracking.Interfaces/NTPAC.ConversationTracking.Interfaces.csproj NTPAC.ConversationTracking.Interfaces/
COPY NTPAC.ConversationTracking.Models/NTPAC.ConversationTracking.Models.csproj NTPAC.ConversationTracking.Models/
COPY NTPAC.ConversationTracking/NTPAC.ConversationTracking.csproj NTPAC.ConversationTracking/
COPY NTPAC.LoadBalancer.Actors/NTPAC.LoadBalancer.Actors.csproj NTPAC.LoadBalancer.Actors/
COPY NTPAC.LoadBalancer.Interfaces/NTPAC.LoadBalancer.Interfaces.csproj NTPAC.LoadBalancer.Interfaces/
COPY NTPAC.LoadBalancer.Messages/NTPAC.LoadBalancer.Messages.csproj NTPAC.LoadBalancer.Messages/
COPY NTPAC.LoadBalancer.RawPacketParser/NTPAC.LoadBalancer.RawPacketParser.csproj NTPAC.LoadBalancer.RawPacketParser/
COPY NTPAC.LoadBalancer/NTPAC.LoadBalancer.csproj NTPAC.LoadBalancer/
COPY NTPAC.LoadBalancerCli/NTPAC.LoadBalancerCli.csproj NTPAC.LoadBalancerCli/
COPY NTPAC.Messages/NTPAC.ConversationTracking.Messages.csproj NTPAC.Messages/
COPY NTPAC.PcapLoader/NTPAC.PcapLoader.csproj NTPAC.PcapLoader/
COPY NTPAC.Persistence.Cassandra.Facades/NTPAC.Persistence.Cassandra.Facades.csproj NTPAC.Persistence.Cassandra.Facades/
COPY NTPAC.Persistence.DevNull.Facades/NTPAC.Persistence.DevNull.Facades.csproj NTPAC.Persistence.DevNull.Facades/
COPY NTPAC.Persistence.DTO/NTPAC.Persistence.DTO.csproj NTPAC.Persistence.DTO/
COPY NTPAC.Persistence.Entities/NTPAC.Persistence.Entities.csproj NTPAC.Persistence.Entities/
COPY NTPAC.Persistence.Generic.Facades/NTPAC.Persistence.Generic.Facades.csproj NTPAC.Persistence.Generic.Facades/
COPY NTPAC.Persistence.InMemory.Facades/NTPAC.Persistence.InMemory.Facades.csproj NTPAC.Persistence.InMemory.Facades/
COPY NTPAC.Persistence.Interfaces/NTPAC.Persistence.Interfaces.csproj NTPAC.Persistence.Interfaces/
COPY NTPAC.Persistence.Models/NTPAC.Persistence.Models.csproj NTPAC.Persistence.Models/
COPY NTPAC.Persistence.Pcap.Facade/NTPAC.Persistence.Pcap.Facade.csproj NTPAC.Persistence.Pcap.Facade/
COPY NTPAC.Reassembling/NTPAC.Reassembling.csproj NTPAC.Reassembling/
COPY Snoopers/SnooperDNS/SnooperDNS.csproj Snoopers/SnooperDNS/
COPY Snoopers/SnooperHTTP/SnooperHTTP.csproj Snoopers/SnooperHTTP/
RUN NUGET_USER=$nuget_user NUGET_PASS=$nuget_pass dotnet restore NTPAC.LoadBalancerCli/
COPY . .
WORKDIR /src/NTPAC.LoadBalancerCli
RUN dotnet publish -c Release --no-restore -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
COPY NTPAC.LoadBalancerCli/docker-entrypoint.sh .
ENTRYPOINT ["sh", "docker-entrypoint.sh"]
