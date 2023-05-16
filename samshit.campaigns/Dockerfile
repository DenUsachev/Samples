#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 8890
EXPOSE 10000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY . .
RUN dotnet restore --configfile "CampaignsService/Campaigns.Api.Web/NuGet.Config" "CampaignsService/Campaigns.Api.Web/Campaigns.Api.Web.csproj" 
WORKDIR "/src/CampaignsService/Campaigns.Api.Web"
RUN dotnet build "Campaigns.Api.Web.csproj" -c Debug -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Campaigns.Api.Web.csproj" -c Debug -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Campaigns.Api.Web.dll"]