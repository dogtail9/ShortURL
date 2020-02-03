docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P@ssw0rd' -p 1435:1433 -d --name shorturlcache mcr.microsoft.com/mssql/server:2019-GDR1-ubuntu-16.04
Start-Sleep -Seconds 20
docker exec shorturlcache /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "P@ssw0rd" -Q "CREATE DATABASE ShortUrlCache"
dotnet sql-cache create "Server=localhost,1435;Database=ShortUrlCache;User Id=sa;Password=P@ssw0rd;" dbo UrlCache
docker stop shorturlcache
docker commit shorturlcache shorturl/cache:v0.1
docker rm shorturlcache