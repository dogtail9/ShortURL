kubectl apply -f ./010_namespace.yml
kubectl apply -f ./020_gateway.yml
kubectl apply -f ./030_sts.yml
kubectl apply -f ./040_zipkin.yml
kubectl apply -f ./050_database.yml
kubectl apply -f ./060_redirectApi.yml
kubectl apply -f ./070_managementApi.yml
kubectl apply -f ./080_adminWeb.yml
