docker build --build-arg http_proxy=http://devproxy.shbmain.shb.biz:8088 -f ShortUrl.IdentityServer/Dockerfile -t shorturl.identityserver:latest .
docker build --build-arg http_proxy=http://devproxy.shbmain.shb.biz:8088 -f ShortUrl.ManagementGui/Dockerfile -t shorturl.managementgui:latest .
docker build --build-arg http_proxy=http://devproxy.shbmain.shb.biz:8088 -f ShortUrl.RedirectApi/Dockerfile -t shorturl.redirectapi:latest .
docker build --build-arg http_proxy=http://devproxy.shbmain.shb.biz:8088 -f ShortUrl.UrlManagementApi/Dockerfile -t shorturl.urlmanagementapi:latest .