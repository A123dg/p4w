FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["p4w.Api/p4w.Api.csproj", "p4w.Api/"]
COPY ["p4w.Core/p4w.Core.csproj", "p4w.Core/"]
COPY ["p4w.Service/p4w.Service.csproj", "p4w.Service/"]
COPY ["p4w.Data/p4w.Data.csproj", "p4w.Data/"]
RUN dotnet restore "p4w.Api/p4w.Api.csproj"

COPY . .
WORKDIR /src/p4w.Api
RUN dotnet publish "p4w.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "p4w.Api.dll"]
