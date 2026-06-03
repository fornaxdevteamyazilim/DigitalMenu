# Railway production — Digital Menu

Güncel API adresi: **https://digitalmenu-production-72f0.up.railway.app**

Aşağıdaki değişkenleri Railway dashboard → ilgili servis → **Variables** (ve gerekiyorsa **Build** argümanları) alanına kopyalayın. GitHub’a push sonrası servisler otomatik yeniden deploy olur.

## API servisi

| Değişken | Değer |
|----------|--------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `DATABASE_URL` | `${{Postgres.DATABASE_URL}}` (Postgres eklentisi referansı) |
| `QrMenu__BaseUrl` | QR menü domain’i belli olunca: `https://<qrmenu-host>/r` |
| `CORS_ALLOWED_ORIGINS` | Admin + QR public URL’leri (virgülle, `https` ile) |

`CORS_ALLOWED_ORIGINS` örneği (domain’leri kendi Railway URL’lerinizle değiştirin):

```
https://<admin-host>.up.railway.app,https://<qrmenu-host>.up.railway.app
```

Sağlık kontrolü: https://digitalmenu-production-72f0.up.railway.app/health

## QR Menü servisi

| Ayar | Değer |
|------|--------|
| Config file | `railway.qrmenu.toml` |
| Dockerfile | `src/DigitalMenu.QrMenu/Dockerfile` |
| Root Directory | *(boş)* |

| Değişken / build arg | Değer |
|----------------------|--------|
| `VITE_API_BASE_URL` | `https://digitalmenu-production-72f0.up.railway.app` |

Deploy sonrası public URL’yi alın → API’de `QrMenu__BaseUrl` = `https://<qrmenu-host>/r` yapıp **API’yi yeniden deploy** edin.

## Admin Panel servisi

| Ayar | Değer |
|------|--------|
| Config file | `src/DigitalMenu.AdminPanel/railway.toml` |
| Dockerfile | `src/DigitalMenu.AdminPanel/Dockerfile` |
| Root Directory | *(boş)* |

| Build arg / Variable | Değer |
|----------------------|--------|
| `API_BASE_URL` | `https://digitalmenu-production-72f0.up.railway.app` (build **ve** runtime — container start’ta `appsettings.json` güncellenir) |

Deploy sonrası public URL’yi `CORS_ALLOWED_ORIGINS` listesine ekleyip **API’yi yeniden deploy** edin.

## Sıra

1. API değişkenleri (`DATABASE_URL`, `ASPNETCORE_ENVIRONMENT`)
2. QR Menü deploy → `VITE_API_BASE_URL` → API’de `QrMenu__BaseUrl`
3. Admin deploy → `API_BASE_URL` → API’de `CORS_ALLOWED_ORIGINS`
