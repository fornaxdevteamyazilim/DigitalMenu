# DigitalMenu.AdminPanel (Blazor WASM) — statik nginx
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DigitalMenu.AdminPanel/DigitalMenu.AdminPanel.csproj", "DigitalMenu.AdminPanel/"]
COPY ["DigitalMenu.Core/DigitalMenu.Core.csproj", "DigitalMenu.Core/"]

RUN dotnet restore "DigitalMenu.AdminPanel/DigitalMenu.AdminPanel.csproj"

COPY DigitalMenu.AdminPanel/ DigitalMenu.AdminPanel/
COPY DigitalMenu.Core/ DigitalMenu.Core/

# Üretim API adresi: build arg ile wwwroot/appsettings.json güncellenir
ARG API_BASE_URL=https://localhost:7182
RUN printf '{\n  "ApiBaseUrl": "%s"\n}\n' "$API_BASE_URL" > DigitalMenu.AdminPanel/wwwroot/appsettings.json \
    && dotnet publish "DigitalMenu.AdminPanel/DigitalMenu.AdminPanel.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
COPY DigitalMenu.AdminPanel/nginx-spa.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
