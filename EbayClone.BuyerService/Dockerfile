# ./EbayClone.BuyerService/Dockerfile
# ---- Base build image ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy 
COPY . .

# Restore 
WORKDIR /src/EbayClone.BuyerService
RUN dotnet restore "EbayClone.BuyerService.csproj"

# Publish app
RUN dotnet publish "EbayClone.BuyerService.csproj" -c Release -o /app/publish

# ---- Final runtime image ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EbayClone.BuyerService.dll"]

