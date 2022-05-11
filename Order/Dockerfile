#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
ENV CONNECTION_STRING = mongodb://db-mongo:27017
ENV RABBITMQ_PORT 5672
ENV RABBITMQ_HOST rabbitmq
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Order.csproj", "."]
RUN dotnet restore "./Order.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Order.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Order.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Order.dll"]