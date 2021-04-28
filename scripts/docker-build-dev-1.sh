timestamp=`date +%s`
docker build -t 10.1.1.11:32000/chabloom-billing-backend:$timestamp -t 10.1.1.11:32000/chabloom-billing-backend:latest .
docker push 10.1.1.11:32000/chabloom-billing-backend:$timestamp
docker push 10.1.1.11:32000/chabloom-billing-backend:latest
