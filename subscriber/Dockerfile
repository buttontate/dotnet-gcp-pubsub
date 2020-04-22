FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder

WORKDIR /app

COPY . ./

RUN dotnet build -c Release

RUN dotnet publish -c Release -o out --no-restore

#######################################

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

WORKDIR /app

COPY --from=builder /app/out ./

RUN apk --no-cache update && apk --no-cache upgrade --available && sync

ENTRYPOINT ["dotnet", "subscriber.dll"]