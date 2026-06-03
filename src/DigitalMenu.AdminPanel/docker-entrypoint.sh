#!/bin/sh
set -e
API_URL="${API_BASE_URL:-https://digitalmenu-production-72f0.up.railway.app}"
API_URL="${API_URL%/}"
printf '%s\n' "{\"ApiBaseUrl\":\"${API_URL}\"}" > /usr/share/nginx/html/appsettings.json
printf '%s\n' "{\"ApiBaseUrl\":\"${API_URL}\"}" > /usr/share/nginx/html/appsettings.Production.json
exec nginx -g 'daemon off;'
