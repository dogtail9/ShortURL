#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
ARG http_proxy

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
ARG http_proxy
ENV http_proxy=$http_proxy
ENV HTTP_PROXY=$http_proxy
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