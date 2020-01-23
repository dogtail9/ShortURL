#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["ShortUrl.IdentityServer/ShortUrl.IdentityServer.csproj", "ShortUrl.IdentityServer/"]
RUN dotnet restore "ShortUrl.IdentityServer/ShortUrl.IdentityServer.csproj"
COPY . .
WORKDIR "/src/ShortUrl.IdentityServer"
RUN dotnet build "ShortUrl.IdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShortUrl.IdentityServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShortUrl.IdentityServer.dll"]