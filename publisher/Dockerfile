FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder

WORKDIR /app

COPY . ./

RUN dotnet build -c Release

RUN dotnet publish -c Release -o out --no-restore

#######################################

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

WORKDIR /app

COPY --from=builder /app/out ./

ENTRYPOINT ["dotnet", "publisher.dll"]