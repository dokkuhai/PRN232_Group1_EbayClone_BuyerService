# ================================
# Build stage
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EbayCloneBuyerService_CoreAPI/*.csproj ./EbayCloneBuyerService_CoreAPI/
RUN dotnet restore ./EbayCloneBuyerService_CoreAPI/EbayCloneBuyerService_CoreAPI.csproj

COPY EbayCloneBuyerService_CoreAPI/. ./EbayCloneBuyerService_CoreAPI/
WORKDIR /src/EbayCloneBuyerService_CoreAPI

RUN dotnet publish -c Release -o /app/publish

# ================================
# Runtime stage
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8081
ENTRYPOINT ["dotnet", "EbayClone.BuyerService.CoreAPI.dll"]
