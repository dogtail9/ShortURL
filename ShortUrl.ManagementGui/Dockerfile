#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
ARG http_proxy

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
ARG http_proxy
ENV http_proxy=$http_proxy
ENV HTTP_PROXY=$http_proxy
WORKDIR /src
COPY ["nuget.config", "nuget.config"]
COPY ["ShortUrl.ManagementGui/ShortUrl.ManagementGui.csproj", "ShortUrl.ManagementGui/"]
RUN dotnet restore "ShortUrl.ManagementGui/ShortUrl.ManagementGui.csproj"
COPY . .
WORKDIR "/src/ShortUrl.ManagementGui"
RUN dotnet build "ShortUrl.ManagementGui.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShortUrl.ManagementGui.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShortUrl.ManagementGui.dll"]