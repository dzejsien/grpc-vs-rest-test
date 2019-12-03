FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o publish-build

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /publish-runtime

COPY --from=build /app/publish .

COPY cert.crt /usr/local/share/ca-certificates/cert.crt
RUN chmod 644 /usr/local/share/ca-certificates/cert.crt && update-ca-certificates

ENTRYPOINT ["dotnet","gRpc.Vs.WebApi.Gateway.dll"]
