# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WeatherDashboardBackend/WeatherDashboardBackend.csproj", "WeatherDashboardBackend/"]
RUN dotnet restore "./WeatherDashboardBackend/WeatherDashboardBackend.csproj"
COPY . .
WORKDIR "/src/WeatherDashboardBackend"
RUN dotnet build "./WeatherDashboardBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WeatherDashboardBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "WeatherDashboardBackend.dll"]
