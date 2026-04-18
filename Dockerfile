# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Vista.Core/Vista.Core.csproj Vista.Core/
RUN dotnet restore Vista.Core/Vista.Core.csproj

COPY Vista.Core/ Vista.Core/
RUN dotnet publish Vista.Core/Vista.Core.csproj -c Release -o /app/publish

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Storage ve Logs klasörleri
RUN mkdir -p /app/Storage /app/Logs

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Vista.Core.dll"]
