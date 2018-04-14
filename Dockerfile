FROM microsoft/dotnet:2.0-sdk

WORKDIR /opt/nist-formatter

COPY ./src /opt/nist-formatter/src

RUN apt-get update && apt-get install -y unzip &&\
	apt-get update && apt-get install -y wget &&\
	apt-get update && apt-get install -y libgdiplus &&\
	wget -O data.zip https://ams3.digitaloceanspaces.com/nist-database19/by_class.zip &&\
	unzip -q data.zip &&\
	rm data.zip &&\
	mv ./by_class ./data &&\
	dotnet publish -r ubuntu.16.04-x64 -o ./publish ./src/nist-formatter-core/NistFormatter &&\
	chmod 777 ./src/nist-formatter-core/NistFormatter/NistFormatter/publish/NistFormatter
	
CMD cd ./src/nist-formatter-core/NistFormatter/NistFormatter/publish && ./NistFormatter