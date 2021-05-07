timestamp=`date +%s`
docker build -t chb-uat-1.chabloom.com:32000/chabloom-billing-backend:$timestamp -t chb-uat-1.chabloom.com:32000/chabloom-billing-backend:latest .
docker push chb-uat-1.chabloom.com:32000/chabloom-billing-backend:$timestamp
docker push chb-uat-1.chabloom.com:32000/chabloom-billing-backend:latest
