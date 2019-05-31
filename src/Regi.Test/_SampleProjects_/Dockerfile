FROM mcr.microsoft.com/dotnet/core/sdk:2.2

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - \
  && apt-get update \
  && DEBIAN_FRONTEND=noninteractive apt-get install -y \
    nodejs chromium net-tools build-essential psmisc \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

ENV PUPPETEER_EXECUTABLE_PATH /usr/bin/chromium

COPY . / src/

RUN cd src \
  && dotnet tool install -g regi --add-source ./

ENV PATH="${PATH}:/root/.dotnet/tools"
