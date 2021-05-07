timestamp=`date +%s`
docker build -t chb-dev-1.chabloom.com:32000/chabloom-billing-backend:$timestamp -t chb-dev-1.chabloom.com:32000/chabloom-billing-backend:latest .
docker push chb-dev-1.chabloom.com:32000/chabloom-billing-backend:$timestamp
docker push chb-dev-1.chabloom.com:32000/chabloom-billing-backend:latest
