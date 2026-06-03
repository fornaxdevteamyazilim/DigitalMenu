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

ARG API_BASE_URL=https://digitalmenu-production-72f0.up.railway.app
RUN API_URL="${API_BASE_URL%/}" \
    && printf '%s\n' "{\"ApiBaseUrl\":\"${API_URL}\"}" > DigitalMenu.AdminPanel/wwwroot/appsettings.Production.json \
    && dotnet publish "DigitalMenu.AdminPanel/DigitalMenu.AdminPanel.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
ENV PORT=8080
ENV API_BASE_URL=https://digitalmenu-production-72f0.up.railway.app
COPY src/DigitalMenu.AdminPanel/nginx-spa.conf /etc/nginx/conf.d/default.conf
COPY src/DigitalMenu.AdminPanel/docker-entrypoint.sh /docker-entrypoint.sh
RUN sed -i 's/\r$//' /docker-entrypoint.sh && chmod +x /docker-entrypoint.sh
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 8080
ENTRYPOINT ["/docker-entrypoint.sh"]
