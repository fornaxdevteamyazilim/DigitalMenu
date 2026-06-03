# DigitalMenu.API — Railway / Docker
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DigitalMenu.API/DigitalMenu.API.csproj", "DigitalMenu.API/"]
COPY ["DigitalMenu.Core/DigitalMenu.Core.csproj", "DigitalMenu.Core/"]
COPY ["DigitalMenu.Infrastructure/DigitalMenu.Infrastructure.csproj", "DigitalMenu.Infrastructure/"]

RUN dotnet restore "DigitalMenu.API/DigitalMenu.API.csproj"

COPY DigitalMenu.API/ DigitalMenu.API/
COPY DigitalMenu.Core/ DigitalMenu.Core/
COPY DigitalMenu.Infrastructure/ DigitalMenu.Infrastructure/

RUN dotnet publish "DigitalMenu.API/DigitalMenu.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DigitalMenu.API.dll"]
