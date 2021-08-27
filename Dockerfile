FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./src/AutoDeployment/AutoDeployment.csproj ./src/AutoDeployment/AutoDeployment.csproj
COPY ./src/GitLabApiClient/GitLabApiClient.csproj ./src/GitLabApiClient/GitLabApiClient.csproj

# Add other csproj files above
RUN dotnet restore ./src/AutoDeployment -s https://api.nuget.org/v3/index.json

# Copy sources and build
COPY ./src/. ./src/

# build
RUN dotnet publish ./src/AutoDeployment -r linux-x64 -o out

# Create runtime image
FROM kroniak/ssh-client
WORKDIR /app


COPY --from=build-env /app/out/ .
RUN mkdir -p ~/.ssh
RUN echo -e "Host *\n\tStrictHostKeyChecking no\n\n" > ~/.ssh/config
RUN cp ./id_rsa ~/.ssh/
RUN chmod 600 ~/.ssh/id_rsa
RUN ssh pi@nordic.preclikos.cz -p 2200 sudo systemctl stop financebot
RUN scp -P 2200 -r /app/* pi@nordic.preclikos.cz:/home/pi/bot
RUN ssh pi@nordic.preclikos.cz -p 2200 sudo systemctl start financebot
