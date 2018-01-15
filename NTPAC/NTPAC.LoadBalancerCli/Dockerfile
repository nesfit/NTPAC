FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY *.sln ./
COPY NTPAC.LoadBalancerCli/NTPAC.LoadBalancerCli.csproj NTPAC.LoadBalancerCli/
COPY NTPAC.PcapLoader/NTPAC.PcapLoader.csproj NTPAC.PcapLoader/
COPY NTPAC.Common/NTPAC.Common.csproj NTPAC.Common/
COPY Libs/sharppcap/Libs/packetnet/PacketDotNet.Interface/PacketDotNet.Interface.csproj Libs/sharppcap/Libs/packetnet/PacketDotNet.Interface/
COPY Libs/sharppcap/SharpPcap/SharpPcap.csproj Libs/sharppcap/SharpPcap/
COPY Libs/sharppcap/Libs/packetnet/PacketDotNet/PacketDotNet.csproj Libs/sharppcap/Libs/packetnet/PacketDotNet/
COPY NTPAC.LoadBalancer/NTPAC.LoadBalancer.csproj NTPAC.LoadBalancer/
COPY NTPAC.Messages/NTPAC.Messages.csproj NTPAC.Messages/
COPY NTPAC.Actors/NTPAC.Actors.csproj NTPAC.Actors/
RUN dotnet restore
COPY . .
WORKDIR /src/NTPAC.LoadBalancerCli
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "NTPAC.LoadBalancerCli.dll"]
