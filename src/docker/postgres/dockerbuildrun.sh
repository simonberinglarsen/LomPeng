docker build -t dbserver .
docker run -itd --net=backend --name=dbserver dbserver
