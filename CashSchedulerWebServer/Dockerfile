FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["CashSchedulerWebServer/CashSchedulerWebServer.csproj", "CashSchedulerWebServer/"]
RUN dotnet restore "CashSchedulerWebServer/CashSchedulerWebServer.csproj"
COPY . .
WORKDIR "/src/CashSchedulerWebServer"
RUN dotnet build "CashSchedulerWebServer.csproj" -c Release -o /app/build
#COPY ["Db/MockData/", "app/Db/MockData/"]

FROM build AS publish
RUN dotnet publish "CashSchedulerWebServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashSchedulerWebServer.dll"]
