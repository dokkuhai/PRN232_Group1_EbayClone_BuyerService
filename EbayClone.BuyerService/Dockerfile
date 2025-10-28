# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy 
COPY . .

WORKDIR /src/EbayClone.BuyerService

# Restore dependencies
RUN dotnet restore "EbayClone.BuyerService.csproj"

# Publish project
RUN dotnet publish "EbayClone.BuyerService.csproj" -c Release -o /app/publish

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EbayClone.BuyerService.dll"]
