FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic
ARG NUGET_KEY
WORKDIR /src
COPY . .
RUN dotnet build ConvertApi
RUN dotnet pack --output . ConvertApi
RUN dotnet nuget push ConvertApi.*.nupkg --api-key="$NUGET_KEY" --source="https://api.nuget.org/v3/index.json" --skip-duplicate