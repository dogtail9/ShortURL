kubectl label namespace default istio-injection=enabled
kubectl get namespace -L istio-injection
kubectl apply -f istio-ingress-1.1.2.yaml