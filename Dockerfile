FROM microsoft/dotnet:2.0-sdk

WORKDIR /opt/nist-formatter

COPY ./src /opt/nist-formatter/src

RUN apt-get install -y wget unzip &&\
	wget -O data.zip https://nist-database19.ams3.digitaloceanspaces.com/by_class.zip &&\
	unzip data.zip &&\
	rm data.zip &&\
	mv ./by_class ./data &&\
	dotnet publish -r ubuntu.16.04-x64 -o ./publish ./src/nist-formatter-core/NistFormatter &&\
	chmod 777 ./src/nist-formatter-core/NistFormatter/NistFormatter/publish/NistFormatter
	
CMD ./src/nist-formatter-core/NistFormatter/NistFormatter/publish/NistFormatter