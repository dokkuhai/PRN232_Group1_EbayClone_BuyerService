# ================================
# Build stage
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EbayClone.BuyerService.CoreAPI/*.csproj ./EbayClone.BuyerService.CoreAPI/
RUN dotnet restore ./EbayClone.BuyerService.CoreAPI/EbayClone.BuyerService.CoreAPI.csproj

COPY EbayClone.BuyerService.CoreAPI/. ./EbayClone.BuyerService.CoreAPI/
WORKDIR /src/EbayClone.BuyerService.CoreAPI

RUN dotnet publish -c Release -o /app/publish

# ================================
# Runtime stage
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8081
ENTRYPOINT ["dotnet", "EbayClone.BuyerService.CoreAPI.dll"]
