# Railway production — Digital Menu

| Servis | URL |
|--------|-----|
| **API** | https://digitalmenu-production-72f0.up.railway.app |
| **QR Menü** | https://triumphant-abundance-production-1cb2.up.railway.app |
| **Admin** | *(Railway public URL’nizi ekleyin)* |

## API servisi — Variables

| Değişken | Değer |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `DATABASE_URL` | `${{Postgres.DATABASE_URL}}` |
| `QrMenu__BaseUrl` | `https://triumphant-abundance-production-1cb2.up.railway.app/r` |
| `CORS_ALLOWED_ORIGINS` | Aşağıdaki satır (admin URL’nizi ekleyin) |

`CORS_ALLOWED_ORIGINS` (admin host’unuzu virgülle ekleyin):

```
https://triumphant-abundance-production-1cb2.up.railway.app,https://<admin-host>.up.railway.app
```

Sağlık: https://digitalmenu-production-72f0.up.railway.app/health

API redeploy sonrası masa QR linkleri otomatik güncellenir.

Örnek QR: https://triumphant-abundance-production-1cb2.up.railway.app/r/lezzet-duragi?table=Masa+1

## QR Menü servisi

| Ayar | Değer |
|------|--------|
| Public URL | https://triumphant-abundance-production-1cb2.up.railway.app |
| Config file | `railway.qrmenu.toml` |
| `VITE_API_BASE_URL` | `https://digitalmenu-production-72f0.up.railway.app` |

## Admin Panel servisi

| Değişken | Değer |
|----------|--------|
| `API_BASE_URL` | `https://digitalmenu-production-72f0.up.railway.app` |
| Config file | `src/DigitalMenu.AdminPanel/railway.toml` |

Admin deploy sonrası public URL’yi `CORS_ALLOWED_ORIGINS` içine ekleyip **API’yi yeniden deploy** edin.
