FROM mcr.microsoft.com/dotnet/core/sdk:2.2

ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

# ######
# ## Install developer tools
# ######
RUN apt-get update \
  && apt-get install -y telnet build-essential \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

# ######
# ## Install NodeJS
# ######
ENV NODE_VERSION 8.9.4
ENV NODE_DOWNLOAD_SHA 21fb4690e349f82d708ae766def01d7fec1b085ce1f5ab30d9bda8ee126ca8fc
RUN curl -SL "https://nodejs.org/dist/v${NODE_VERSION}/node-v${NODE_VERSION}-linux-x64.tar.gz" --output nodejs.tar.gz --insecure \
    && echo "$NODE_DOWNLOAD_SHA nodejs.tar.gz" | sha256sum -c - \
    && tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
    && rm nodejs.tar.gz \
    && ln -s /usr/local/bin/node /usr/local/bin/nodejs
    
# ######
# ## Install Chromium
# ######
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - \
  && apt-get update \
  && DEBIAN_FRONTEND=noninteractive apt-get install -y \
    chromium net-tools psmisc \
  && apt-get clean \
  && rm -rf /var/lib/apt/lists/*

# RUN apt-get update \
#   && DEBIAN_FRONTEND=noninteractive apt-get install -y \
#     chromium net-tools build-essential psmisc \
#   && apt-get clean \
#   && rm -rf /var/lib/apt/lists/*


ENV PUPPETEER_SKIP_CHROMIUM_DOWNLOAD true
ENV PUPPETEER_EXECUTABLE_PATH /usr/bin/chromium

COPY . / src/

RUN cd src \
  && dotnet tool install -g regi --add-source ./

ENV PATH="${PATH}:/root/.dotnet/tools"
