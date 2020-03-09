FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic
WORKDIR /src
COPY . .
RUN dotnet build ConvertApi
RUN dotnet pack --output . ConvertApi
RUN echo $NUGET_KEY
RUN dotnet nuget push ConvertApi.*.nupkg --api-key="$NUGET_KEY" --source="https://api.nuget.org/v3/index.json"