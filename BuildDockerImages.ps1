docker build -f ShortUrl.IdentityServer/Dockerfile -t shorturl.identityserver:latest .
docker build -f ShortUrl.ManagementGui/Dockerfile -t shorturl.managementgui:latest .
docker build -f ShortUrl.RedirectApi/Dockerfile -t shorturl.redirectapi:latest .
docker build -f ShortUrl.UrlManagementApi/Dockerfile -t shorturl.urlmanagementapi:latest .