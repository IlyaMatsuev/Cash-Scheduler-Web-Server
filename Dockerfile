FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["CashSchedulerWebServer/CashSchedulerWebServer.csproj", "CashSchedulerWebServer/"]
RUN dotnet restore "CashSchedulerWebServer/CashSchedulerWebServer.csproj"
COPY . .
WORKDIR "/src/CashSchedulerWebServer"
RUN dotnet build "CashSchedulerWebServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CashSchedulerWebServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashSchedulerWebServer.dll"]
