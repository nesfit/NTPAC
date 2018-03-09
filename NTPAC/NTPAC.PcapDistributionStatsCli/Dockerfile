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
COPY NTPAC.Common/NTPAC.Common.csproj NTPAC.Common/
COPY NTPAC.ConversationTracking.Interfaces/NTPAC.ConversationTracking.Interfaces.csproj NTPAC.ConversationTracking.Interfaces/
COPY NTPAC.ConversationTracking.Models/NTPAC.ConversationTracking.Models.csproj NTPAC.ConversationTracking.Models/
COPY NTPAC.LoadBalancer.RawPacketParser/NTPAC.LoadBalancer.RawPacketParser.csproj NTPAC.LoadBalancer.RawPacketParser/
COPY NTPAC.Messages/NTPAC.ConversationTracking.Messages.csproj NTPAC.Messages/
COPY NTPAC.Messages/NTPAC.ConversationTracking.Messages.csproj NTPAC.Messages/
COPY NTPAC.PcapDistributionStatsCli/NTPAC.PcapDistributionStatsCli.csproj NTPAC.PcapDistributionStatsCli/
COPY NTPAC.PcapLoader/NTPAC.PcapLoader.csproj NTPAC.PcapLoader/
RUN NUGET_USER=$nuget_user NUGET_PASS=$nuget_pass dotnet restore NTPAC.PcapDistributionStatsCli/
COPY . .
WORKDIR /src/NTPAC.PcapDistributionStatsCli
RUN dotnet publish -c Release --no-restore -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NTPAC.PcapDistributionStatsCli.dll"]


