FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic
WORKDIR /src
COPY . .
RUN dotnet build ConvertApi
RUN dotnet pack --output . ConvertApi
RUN dotnet nuget push ConvertApi.*.nupkg -k $NUGET_KEY -s https://api.nuget.org/v3/index.json