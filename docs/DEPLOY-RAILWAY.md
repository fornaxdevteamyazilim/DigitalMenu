# Digital Menu — Railway’e yayınlama

Bu rehber üç ayrı Railway servisi kurar: **API**, **Admin Panel** (Blazor WASM) ve **QR Menü** (Vue). PostgreSQL Railway eklentisi ile gelir.

## Mimari

| Servis | Teknoloji | Railway Root Directory | Dockerfile |
|--------|-----------|------------------------|------------|
| `digitalmenu-api` | ASP.NET Core 10 | *(boş — repo kökü)* | `src/DigitalMenu.API/Dockerfile` |
| `digitalmenu-admin` | Blazor WASM + nginx | *(boş — repo kökü)* | `src/DigitalMenu.AdminPanel/Dockerfile` |
| `digitalmenu-qrmenu` | Vue + nginx | `src/DigitalMenu.QrMenu` | `Dockerfile` |
| PostgreSQL | Eklenti | — | — |

## 1. Railway projesi ve veritabanı

1. [railway.app](https://railway.app) üzerinde yeni proje oluşturun.
2. **Add Plugin → PostgreSQL** ekleyin.
3. API servisini oluşturmadan önce Postgres’in `DATABASE_URL` değerini not edin (otomatik enjekte edilir).

### GitHub bağlantısı

- Repo: `https://github.com/fornaxdevteamyazilim/DigitalMenu` (**public**; varsayılan dal: **`main`**).
- **New → GitHub Repo** kullanın; repoyu listeden seçin (önce Railway hesabınıza GitHub yetkisi verin). Ham URL yapıştırma yerine bu yol önerilir.
- Repoyu private tutmak isterseniz: Railway’de **Account Settings → GitHub** ile uygulamayı bağlayıp repoyu listeden seçin (URL yapıştırmayın). Private repo + URL yapıştırma çalışmaz.

## 2. API servisi

1. **New Service → GitHub Repo** → `fornaxdevteamyazilim/DigitalMenu` repoyu seçin.
2. **Settings → Build** (önemli — Railpack hatasını önler):
   - **Builder:** `Dockerfile` (Railpack / Nixpacks değil)
   - **Root Directory:** *(boş bırakın — repo kökü)*  
     `src` yaparsanız `DigitalMenu.Infrastructure: not found` hatası alırsınız; Dockerfile repo kökünden derlenir.
   - **Dockerfile Path:** `src/DigitalMenu.API/Dockerfile`
   - **Config file:** `railway.api.toml` (repo kökü — Admin/QR ile karışmasın diye kök `railway.json` kullanılmıyor)
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
2. **Settings → Build:**
   - **Builder:** `Dockerfile`
   - **Root Directory:** `src/DigitalMenu.QrMenu`
   - **Dockerfile Path:** `Dockerfile`
3. **Config file:** `src/DigitalMenu.QrMenu/railway.toml` (healthcheck `/health`, nginx port **8080**)
4. **Build variables / Docker build args:**

   | Arg | Değer |
   |-----|--------|
   | `VITE_API_BASE_URL` | API domain’iniz (örn. `https://digitalmenu-api.up.railway.app`) |

5. Public domain oluşturun; bu URL’yi API’de `QrMenu__BaseUrl` olarak `https://<qrmenu-domain>/r` şeklinde güncelleyin.

QR örnek link: `https://<qrmenu-domain>/r/lezzet-duragi?table=Masa+1`

## 4. Admin Panel servisi

1. Yeni servis, **Settings → Build:**
   - **Builder:** `Dockerfile`
   - **Root Directory:** *(boş — repo kökü)*
   - **Dockerfile Path:** `src/DigitalMenu.AdminPanel/Dockerfile`
   - **Config file:** `src/DigitalMenu.AdminPanel/railway.toml` *(zorunlu — yoksa kök `railway.api.toml` API Dockerfile’ını kullanır ve healthcheck düşer)*
2. **Settings → Deploy → Healthcheck Path:** `/health` (config dosyasından da gelir)
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
cd c:\projects\Menю
docker build -f src/DigitalMenu.API/Dockerfile -t digitalmenu-api .
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
| “Only public repositories can be added using the GitHub URL” | Repo private veya URL yapıştırılıyor; repoyu public yapın veya GitHub uygulamasıyla listeden seçin |
| “Branch 'main' not found” | Railway’de **Branch** alanını `main` yapın (repo `master` kullanıyorsa önce GitHub’da `main` dalına geçin) |
| `DigitalMen` / yanlış repo adı | Tam ad: **`fornaxdevteamyazilim/DigitalMenu`** (sonunda `u`) |
| `railpack process exited with an error` | **Builder = Dockerfile**, Root *(boş)*, path `src/DigitalMenu.API/Dockerfile` |
| `DigitalMenu.Infrastructure: not found` | Root Directory `src` iken context yanlış; Root’u **boş** bırakın (repo kökü) |
| QR Menü / Admin **healthcheck failure** | nginx **8080** + `/health`; Admin’de **Config file** = `src/DigitalMenu.AdminPanel/railway.toml`. Kök `railway.json` tüm servislere API build’i uygular — kullanmayın |
| API açılmıyor | `DATABASE_URL` veya Postgres referansı eksik |
| CORS hatası | `CORS_ALLOWED_ORIGINS` eksik veya `https` uyumsuzluğu |
| QR menü boş / hata | `VITE_API_BASE_URL` build sırasında set edilmemiş |
| Admin API’ye bağlanamıyor | `API_BASE_URL` build arg yanlış; `wwwroot/appsettings.json` publish’te güncellenir |
| SignalR kopuyor | API ve front aynı `https` origin politikası; CORS + credentials |
