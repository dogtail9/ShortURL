#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["nuget.config", "nuget.config"]
COPY ["ShortUrl.RedirectApi/ShortUrl.RedirectApi.csproj", "ShortUrl.RedirectApi/"]
COPY ["ShortUrl.DataAccess.Sql/ShortUrl.DataAccess.Sql.csproj", "ShortUrl.DataAccess.Sql/"]
RUN dotnet restore "ShortUrl.RedirectApi/ShortUrl.RedirectApi.csproj"
COPY . .
WORKDIR "/src/ShortUrl.RedirectApi"
RUN dotnet build "ShortUrl.RedirectApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShortUrl.RedirectApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShortUrl.RedirectApi.dll"]