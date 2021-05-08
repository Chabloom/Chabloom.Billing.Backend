timestamp=`date +%s`
docker build -t mdcasey/chabloom-billing-backend:$timestamp -t mdcasey/chabloom-billing-backend:latest .
docker push mdcasey/chabloom-billing-backend:$timestamp
docker push mdcasey/chabloom-billing-backend:latest
