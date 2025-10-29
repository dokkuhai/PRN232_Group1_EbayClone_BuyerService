# ================================
# Build stage
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY EbayCloneBuyerService_CoreAPI/*.csproj ./EbayCloneBuyerService_CoreAPI/
RUN dotnet restore ./EbayCloneBuyerService_CoreAPI/EbayCloneBuyerService_CoreAPI.csproj

# Copy all source code
COPY EbayCloneBuyerService_CoreAPI/. ./EbayCloneBuyerService_CoreAPI/
WORKDIR /src/EbayCloneBuyerService_CoreAPI

# Publish
RUN dotnet publish -c Release -o /app/publish

# ================================
# Runtime stage
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
EXPOSE 8081

# DLL file
ENTRYPOINT ["dotnet", "EbayCloneBuyerService_CoreAPI.dll"]
