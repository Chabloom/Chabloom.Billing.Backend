timestamp=`date +%s`
docker build -t chb-prod-1.chabloom.com:32000/chabloom-billing-backend:$timestamp -t chb-prod-1.chabloom.com:32000/chabloom-billing-backend:latest .
docker push chb-prod-1.chabloom.com:32000/chabloom-billing-backend:$timestamp
docker push chb-prod-1.chabloom.com:32000/chabloom-billing-backend:latest
