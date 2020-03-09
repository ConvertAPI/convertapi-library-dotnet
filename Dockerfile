FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic
WORKDIR /src
COPY . .
RUN dotnet build ConvertApi
RUN dotnet pack --output . ConvertApi
RUN dotnet nuget push ConvertApi.*.nupkg --api-key $NUGET_KEY --source https://www.nuget.org/api/v2/package