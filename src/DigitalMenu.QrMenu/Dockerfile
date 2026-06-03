# DigitalMenu.QrMenu (Vue + Vite) — statik nginx
# Build context: repository root
# Local: docker build -f src/DigitalMenu.QrMenu/Dockerfile --build-arg VITE_API_BASE_URL=... -t digitalmenu-qrmenu .
FROM node:22-alpine AS build
WORKDIR /app

COPY src/DigitalMenu.QrMenu/package.json src/DigitalMenu.QrMenu/package-lock.json ./
RUN npm install --no-audit --no-fund

COPY src/DigitalMenu.QrMenu/ .

ARG VITE_API_BASE_URL=https://digitalmenu-production-72f0.up.railway.app
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

FROM nginx:alpine AS final
ENV PORT=8080
COPY src/DigitalMenu.QrMenu/nginx-spa.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
