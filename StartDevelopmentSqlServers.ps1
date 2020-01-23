docker run --name shorturldata -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P@ssw0rd' -d -m 2g -p 1433:1433 mcr.microsoft.com/mssql/server:2019-GDR1-ubuntu-16.04
docker run --name shorturlcache -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=P@ssw0rd' -d -m 2g -p 1434:1433 shorturl/cache:v0.1
docker run --name shorturlzipkin -d -p 9411:9411 openzipkin/zipkin-slim
docker run --name shorturljaeger -d -e COLLECTOR_ZIPKIN_HTTP_PORT=9412 -p 5775:5775/udp -p 6831:6831/udp -p 6832:6832/udp -p 5778:5778 -p 16686:16686 -p 14268:14268 -p 9412:9411 jaegertracing/all-in-one:1.16