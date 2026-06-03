# DigitalMenu.AdminPanel (Blazor WASM) — statik nginx
# Build context: repository root
# Local: docker build -f src/DigitalMenu.AdminPanel/Dockerfile --build-arg API_BASE_URL=... -t digitalmenu-admin .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/DigitalMenu.AdminPanel/DigitalMenu.AdminPanel.csproj", "DigitalMenu.AdminPanel/"]
COPY ["src/DigitalMenu.Core/DigitalMenu.Core.csproj", "DigitalMenu.Core/"]

RUN dotnet restore "DigitalMenu.AdminPanel/DigitalMenu.AdminPanel.csproj"

COPY src/DigitalMenu.AdminPanel/ DigitalMenu.AdminPanel/
COPY src/DigitalMenu.Core/ DigitalMenu.Core/

ARG API_BASE_URL=https://localhost:7182
RUN printf '{\n  "ApiBaseUrl": "%s"\n}\n' "$API_BASE_URL" > DigitalMenu.AdminPanel/wwwroot/appsettings.json \
    && dotnet publish "DigitalMenu.AdminPanel/DigitalMenu.AdminPanel.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
# Railway healthcheck ve proxy varsayılan olarak 8080 kullanır
ENV PORT=8080
COPY src/DigitalMenu.AdminPanel/nginx-spa.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
