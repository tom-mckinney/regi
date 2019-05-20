FROM mcr.microsoft.com/dotnet/core/sdk:2.2

# RUN apt-get update \
#   && curl -sL https://deb.nodesource.com/setup_12.x | bash - \
#   && apt-get install -y nodejs telnet build-essential

RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - \
  && apt-get update \
  && DEBIAN_FRONTEND=noninteractive apt-get install -y \
    nodejs chromium net-tools build-essential psmisc \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

ENV PUPPETEER_EXECUTABLE_PATH /usr/bin/chromium

COPY src/ regi/
COPY test.sh test.sh

RUN cd regi/Regi \
  && dotnet tool install -g regi --add-source ./

ENV PATH="${PATH}:/root/.dotnet/tools"
