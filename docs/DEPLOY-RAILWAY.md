# Digital Menu — Railway’e yayınlama

Bu rehber üç ayrı Railway servisi kurar: **API**, **Admin Panel** (Blazor WASM) ve **QR Menü** (Vue). PostgreSQL Railway eklentisi ile gelir.

## Mimari

| Servis | Teknoloji | Railway kök dizini | Dockerfile |
|--------|-----------|----------------------|------------|
| `digitalmenu-api` | ASP.NET Core 10 | `src` | `DigitalMenu.API/Dockerfile` |
| `digitalmenu-admin` | Blazor WASM + nginx | `src` | `DigitalMenu.AdminPanel/Dockerfile` |
| `digitalmenu-qrmenu` | Vue + nginx | `src/DigitalMenu.QrMenu` | `Dockerfile` |
| PostgreSQL | Eklenti | — | — |

## 1. Railway projesi ve veritabanı

1. [railway.app](https://railway.app) üzerinde yeni proje oluşturun.
2. **Add Plugin → PostgreSQL** ekleyin.
3. API servisini oluşturmadan önce Postgres’in `DATABASE_URL` değerini not edin (otomatik enjekte edilir).

## 2. API servisi

1. **New Service → GitHub Repo** → bu repoyu seçin.
2. Servis ayarları:
   - **Root Directory:** `src`
   - **Dockerfile Path:** `DigitalMenu.API/Dockerfile`
3. **Variables** (örnek):

   | Değişken | Açıklama |
   |----------|----------|
   | `ASPNETCORE_ENVIRONMENT` | `Production` |
   | `DATABASE_URL` | Postgres eklentisinden `${{Postgres.DATABASE_URL}}` referansı |
   | `QrMenu__BaseUrl` | QR menü public URL, örn. `https://digitalmenu-qrmenu.up.railway.app/r` |
   | `CORS_ALLOWED_ORIGINS` | Virgülle ayrılmış: admin + qrmenu URL’leri |

   `CORS_ALLOWED_ORIGINS` örneği:

   ```
   https://digitalmenu-admin.up.railway.app,https://digitalmenu-qrmenu.up.railway.app
   ```

4. Deploy sonrası **Settings → Networking → Generate Domain** ile API URL’sini alın (örn. `https://digitalmenu-api.up.railway.app`).
5. Sağlık kontrolü: `GET /health` → `{ "status": "ok" }`.

İlk açılışta EF Core migration ve seed otomatik çalışır (`DbInitializer`).

## 3. QR Menü servisi

1. Yeni servis, aynı repo.
2. **Root Directory:** `src/DigitalMenu.QrMenu`
3. **Dockerfile Path:** `Dockerfile`
4. **Build variables / Docker build args:**

   | Arg | Değer |
   |-----|--------|
   | `VITE_API_BASE_URL` | API domain’iniz (örn. `https://digitalmenu-api.up.railway.app`) |

5. Public domain oluşturun; bu URL’yi API’de `QrMenu__BaseUrl` olarak `https://<qrmenu-domain>/r` şeklinde güncelleyin.

QR örnek link: `https://<qrmenu-domain>/r/lezzet-duragi?table=Masa+1`

## 4. Admin Panel servisi

1. Yeni servis, **Root Directory:** `src`
2. **Dockerfile Path:** `DigitalMenu.AdminPanel/Dockerfile`
3. **Docker build arg:**

   | Arg | Değer |
   |-----|--------|
   | `API_BASE_URL` | API domain (örn. `https://digitalmenu-api.up.railway.app`) |

4. Public domain oluşturun; bu URL’yi API `CORS_ALLOWED_ORIGINS` listesine ekleyip API’yi yeniden deploy edin.

## 5. Sıra ve doğrulama

Önerilen sıra:

1. PostgreSQL + API (CORS’u admin/qrmenu URL’leri belli olduktan sonra tekrar deploy edebilirsiniz)
2. QR Menü → `QrMenu__BaseUrl` güncelle → API redeploy
3. Admin Panel → CORS güncelle → API redeploy

Kontrol listesi:

- [ ] `https://<api>/health` çalışıyor
- [ ] Admin panelden masa listesi yükleniyor
- [ ] QR menüde ürünler listeleniyor
- [ ] Garson çağır / hesap iste SignalR ile admin panelde görünüyor
- [ ] Yeni masa QR linki `QrMenu__BaseUrl` ile üretiliyor

## Yerel Docker ile API testi

```powershell
cd c:\projects\Menю\src
docker build -f DigitalMenu.API/Dockerfile -t digitalmenu-api .
docker run --rm -p 8080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=digital_menu_saas;Username=menu_user;Password=MenuSecurePassword2026!" `
  -e CORS_ALLOWED_ORIGINS="http://localhost:5173,http://localhost:5140" `
  digitalmenu-api
```

## Güvenlik notları

- `appsettings.json` içindeki yerel şifreleri üretimde kullanmayın; Railway Variables kullanın.
- Trendyol API anahtarlarını Railway secret olarak saklayın.
- İleride özel domain ve HTTPS Railway tarafında otomatik sağlanır.

## Sorun giderme

| Belirti | Olası neden |
|---------|-------------|
| API açılmıyor | `DATABASE_URL` veya Postgres referansı eksik |
| CORS hatası | `CORS_ALLOWED_ORIGINS` eksik veya `https` uyumsuzluğu |
| QR menü boş / hata | `VITE_API_BASE_URL` build sırasında set edilmemiş |
| Admin API’ye bağlanamıyor | `API_BASE_URL` build arg yanlış; `wwwroot/appsettings.json` publish’te güncellenir |
| SignalR kopuyor | API ve front aynı `https` origin politikası; CORS + credentials |
