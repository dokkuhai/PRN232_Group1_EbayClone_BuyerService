# ./EbayClone.BuyerService/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "EbayClone.BuyerService/EbayClone.BuyerService.csproj"
RUN dotnet publish "EbayClone.BuyerService/EbayClone.BuyerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EbayClone.BuyerService.dll"]
