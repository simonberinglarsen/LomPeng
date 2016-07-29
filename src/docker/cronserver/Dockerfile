FROM ubuntu:latest
RUN 	apt-get update && \
	apt-get install cron -y && \
	apt-get install wget -y && \
	apt-get install nano && \
	export EDITOR=nano

# Add crontab file in the cron directory
COPY ./data/ /data
# Run the command on container startup
CMD crontab /data/crontabfile && service cron start && /bin/bash
