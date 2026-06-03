# Railway production — Digital Menu

| Servis | URL |
|--------|-----|
| **API** | https://digitalmenu-production-72f0.up.railway.app |
| **Admin Panel** | https://natural-gentleness-production-c70d.up.railway.app |
| **QR Menü** | https://triumphant-abundance-production-1cb2.up.railway.app |

## API servisi — Variables

| Değişken | Değer |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `DATABASE_URL` | `${{Postgres.DATABASE_URL}}` |
| `QrMenu__BaseUrl` | `https://triumphant-abundance-production-1cb2.up.railway.app/r` |
| `CORS_ALLOWED_ORIGINS` | `https://natural-gentleness-production-c70d.up.railway.app,https://triumphant-abundance-production-1cb2.up.railway.app` |

Sağlık: https://digitalmenu-production-72f0.up.railway.app/health

Örnek QR: https://triumphant-abundance-production-1cb2.up.railway.app/r/lezzet-duragi?table=Masa+1

## QR Menü servisi

| Değişken | Değer |
|----------|--------|
| `VITE_API_BASE_URL` | `https://digitalmenu-production-72f0.up.railway.app` |
| Config file | `railway.qrmenu.toml` |

## Admin Panel servisi

| Değişken | Değer |
|----------|--------|
| `API_BASE_URL` | `https://digitalmenu-production-72f0.up.railway.app` |
| Config file | `src/DigitalMenu.AdminPanel/railway.toml` |
| Public URL | https://natural-gentleness-production-c70d.up.railway.app |

## Deploy sırası

1. API variables + redeploy (QR URL + CORS)
2. Admin + QR redeploy (son commit — nginx wasm + index.html düzeltmeleri)
