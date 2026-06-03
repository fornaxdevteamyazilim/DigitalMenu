# DigitalMenu.API — Railway / Docker
# Build context: repository root (Railway uses railway.json + src/DigitalMenu.API/Dockerfile)
# Local: docker build -f src/DigitalMenu.API/Dockerfile -t digitalmenu-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/DigitalMenu.API/DigitalMenu.API.csproj", "DigitalMenu.API/"]
COPY ["src/DigitalMenu.Core/DigitalMenu.Core.csproj", "DigitalMenu.Core/"]
COPY ["src/DigitalMenu.Infrastructure/DigitalMenu.Infrastructure.csproj", "DigitalMenu.Infrastructure/"]

RUN dotnet restore "DigitalMenu.API/DigitalMenu.API.csproj"

COPY src/DigitalMenu.API/ DigitalMenu.API/
COPY src/DigitalMenu.Core/ DigitalMenu.Core/
COPY src/DigitalMenu.Infrastructure/ DigitalMenu.Infrastructure/

RUN dotnet publish "DigitalMenu.API/DigitalMenu.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DigitalMenu.API.dll"]
