if((Test-Path ./Dashboard.yaml -PathType Leaf) -ne $true)
{
  Invoke-WebRequest -Uri https://raw.githubusercontent.com/kubernetes/dashboard/v2.0.0/aio/deploy/recommended.yaml -OutFile ./Dashboard.yaml
}

kubectl apply -f ./Dashboard.yaml
kubectl describe secret -n kube-system
kubectl proxy