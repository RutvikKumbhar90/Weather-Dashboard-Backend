# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WeatherDashboardBackend.csproj", "./"]
RUN dotnet restore "./WeatherDashboardBackend.csproj"
COPY . .
RUN dotnet build "WeatherDashboardBackend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 2: Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WeatherDashboardBackend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "WeatherDashboardBackend.dll"]
