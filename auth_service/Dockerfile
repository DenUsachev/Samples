#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 8890

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY . .
RUN dotnet restore --configfile "Samshit.AuthGateway/NuGet.Config" "Samshit.AuthGateway/Samshit.AuthGateway.csproj" 
WORKDIR "/src/Samshit.AuthGateway"
RUN dotnet build "Samshit.AuthGateway.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Samshit.AuthGateway.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Samshit.AuthGateway.dll"]