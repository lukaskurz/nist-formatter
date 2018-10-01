FROM microsoft/dotnet:2.0-sdk as builder

WORKDIR /opt/nist-formatter

COPY ./src /opt/nist-formatter/src
COPY ./appsettings.json /opt/nist-formatter/src/nist-formatter-core/NistFormatter/NistFormatter/appsettings.json

RUN dotnet publish -c Release -r ubuntu.16.04-x64 -o ./publish ./src/nist-formatter-core/NistFormatter &&\
	chmod 777 ./src/nist-formatter-core/NistFormatter/NistFormatter/publish/NistFormatter

FROM ubuntu:latest as downloader

WORKDIR /opt/download

RUN echo -e "\nINSTALLING ADDITIONAL TOOLS\n" &&\
	apt-get update && apt-get install -y curl &&\
	apt-get update && apt-get install -y libgdiplus &&\
	apt-get update && apt-get install -y unzip &&\
	echo "\nDOWNLOADING THE COMPRESSED DATASET\n" &&\
	curl -o data.zip https://ams3.digitaloceanspaces.com/nist-database19/by_class.zip --progress-bar &&\
	echo "\nDECOMPRESSING THE DATASET\n" &&\
	unzip data.zip &&\
	rm data.zip &&\
	mv ./by_class ./data

FROM alpine:latest as execution

WORKDIR /opt/nist-formatter

COPY --from=builder /opt/nist-formatter/src/nist-formatter-core/NistFormatter/NistFormatter/publish ./
COPY --from=downloader /opt/download/data ./data

RUN chmod 777 ./NistFormatter

CMD ./NistFormatter