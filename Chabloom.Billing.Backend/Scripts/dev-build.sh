docker build -t chabloom-billing-backend:1.0.0 .
docker save chabloom-billing-backend > chabloom-billing-backend.tar
microk8s ctr image import chabloom-billing-backend.tar
rm chabloom-billing-backend.tar
