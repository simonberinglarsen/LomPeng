docker build -t dbclient .
docker run -itd --net=backend --name=dbclient dbclient
