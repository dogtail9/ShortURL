docker run --name shorturldata -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P@ssw0rd' -d -m 2g -p 1433:1433 mcr.microsoft.com/mssql/server:2019-GDR1-ubuntu-16.04
docker run --name shorturlcache -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P@ssw0rd' -d -m 2g -p 1434:1433 shorturl/cache:v0.1
docker run --name shorturlzipkin -d -p 9411:9411 openzipkin/zipkin-slim