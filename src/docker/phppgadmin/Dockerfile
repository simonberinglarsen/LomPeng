FROM ubuntu

COPY ./data /data2
WORKDIR /data2

RUN apt-get update && \
	apt-get install apache2 -y && \
	apt-get install phppgadmin -y && \
	apt-get install nano -y && \
	echo "ServerName localhost" >> /etc/apache2/apache2.conf

CMD /bin/sh /data2/run.sh && /bin/bash
