FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build-env

COPY ../ /app

# WORKDIR /app/src/samples/GettingStarted_Application/Api
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Api.dll"]
