#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ConsumerOrders/ConsumerOrders.csproj", "ConsumerOrders/"]
RUN dotnet restore "ConsumerOrders/ConsumerOrders.csproj"
COPY . .
WORKDIR "/src/ConsumerOrders"
RUN dotnet build "ConsumerOrders.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsumerOrders.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsumerOrders.dll"]