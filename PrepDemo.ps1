./000_GetIstioCli.ps1
./010_PullDemoImages.ps1
docker-compose -f .\docker-compose.yml -f .\docker-compose.override.yml -f .\docker-compose.proxy.yml build --parallel
./020_CreateCacheImage.ps1
./030_InstallIstio.ps1
./040_InstallDashboard.ps1