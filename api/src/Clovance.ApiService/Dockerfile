# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy solution and project files
COPY ["api/src/Clovance.ApiService/Clovance.ApiService.csproj", "api/src/Clovance.ApiService/"]
COPY ["api/aspire/Clovance.ServiceDefaults/Clovance.ServiceDefaults.csproj", "api/aspire/Clovance.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "api/src/Clovance.ApiService/Clovance.ApiService.csproj"

# Copy all source files
COPY api/ api/

# Build the application
WORKDIR "/src/api/src/Clovance.ApiService"
RUN dotnet build "Clovance.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Clovance.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clovance.ApiService.dll"]
