# https://istio.io/latest/docs/setup/getting-started/
./istioctl.exe install --set profile=demo
kubectl label namespace default istio-injection=enabled
./istioctl.exe analyze