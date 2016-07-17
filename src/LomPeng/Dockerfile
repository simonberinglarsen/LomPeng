FROM microsoft/dotnet:latest

ENV ASPNETCORE_ENVIRONMENT Development
EXPOSE 5004

RUN apt-get update
RUN apt-get install nano

COPY . /app
WORKDIR /app
RUN ["dotnet", "restore"]
ENTRYPOINT dotnet run


