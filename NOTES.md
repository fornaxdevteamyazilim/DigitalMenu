# Dijital Menü SaaS — Proje Notları

> Orijinal plan: `Dijital Menü SaaS — Uygulama Planı`  
> **Platform değişikliği:** Next.js + NestJS + Turborepo → **.NET 10 (Blazor WebAssembly + ASP.NET Core API)**

---

## Genel Bakış

Multi-tenant dijital menü SaaS platformu. İşletmeler Trendyol Go Yemek API kimlik bilgileriyle menülerini senkronize eder, şablon/renk seçer, masa numaralı QR kod üretir. Müşteriler menüden garson çağırır ve hesap ister.

---

## Teknoloji Yığını (.NET)

| Katman | Seçim | Gerekçe |
|--------|-------|---------|
| Solution | **.NET 10 solution** | Mevcut ekosistem ve ROPNG entegrasyon deneyimi |
| Frontend | **Blazor WebAssembly** | Public menü, admin panel, garson paneli — tek C# codebase |
| Backend | **ASP.NET Core Minimal/Controllers API** | REST, SignalR, background sync job'ları |
| Paylaşılan katman | **DigitalMenu.Core** | Domain modelleri, DTO'lar, Trendyol client, menü şablon mantığı |
| Veritabanı | **PostgreSQL + EF Core** | İlişkisel veri, tenant izolasyonu |
| Cache / Real-time | **Redis + SignalR** | Garson/hesap bildirimleri pub/sub |
| Auth | **ASP.NET Core Identity + JWT** (veya Entra ID B2C) | İşletme sahibi girişi, tenant modeli |
| Dosya depolama | **S3 uyumlu (Cloudflare R2 / Azure Blob)** | Logo ve ürün görselleri |
| QR | **QRCoder** veya **SkiaSharp.QrCode** | PNG export |
| Deploy | **Azure App Service / IIS + PostgreSQL** | Kurumsal .NET deploy |

### Mimari Akış

```
Müşteri (QR) → Blazor WASM Public Menü
                    ↓
              ASP.NET Core API
                    ↓
         PostgreSQL + Redis + SignalR
                    ↓
         Trendyol Go Yemek API
```

---

## Proje Yapısı

```
Menю/
├── DigitalMenuSaaS.slnx
├── src/
│   ├── DigitalMenu.API/          # ASP.NET Core — REST + SignalR + sync
│   ├── DigitalMenu.Client/       # Blazor WASM — public menu + admin + staff
│   └── DigitalMenu.Core/         # Domain, DTO, Trendyol client, şablonlar
├── docker-compose.yml            # PostgreSQL + Redis (local dev)
└── NOTES.md
```

---

## Trendyol Go Yemek API Entegrasyonu

**Kimlik bilgileri** (resmi API):
- `supplierId`
- `apiKey`
- `apiSecret`
- `storeId` / restoran kimliği (API'den listelenir)

> Normal Trendyol kullanıcı adı/şifresi desteklenmez. Partner Panel'den API credentials alınır.

**Güvenlik:** Kimlik bilgileri AES-256 ile şifrelenerek DB'de saklanır; asla client'a dönülmez.

**Sync akışı:**
1. Admin panelde "Trendyol Bağla" → kimlik bilgileri doğrulanır (test API çağrısı)
2. Restoran/şube seçimi
3. Arka planda menü çekilir: kategoriler, ürünler, fiyatlar, açıklamalar, modifier/opsiyon grupları, mümkünse görseller
4. Veriler local `Menu` modeline normalize edilir (Trendyol ID'leri `externalId` olarak tutulur)
5. Periyodik sync (6 saatte bir) + manuel "Şimdi Senkronize Et" butonu

**Önemli kısıt:** Trendyol'dan gelen fiyatlar paket/komisyon dahil olabilir → admin panelde **ürün bazlı fiyat override** zorunlu.

**Referans:** [Trendyol Go Yemek API Docs](https://developers.tgoapps.com/docs/category/8-uber-eats-trendyol-go---yemek-entegrasyonu)

**Mevcut referans kod:** `ROPNGModernization/Legacy/NGAPI/ROPNG.Trendyol/` — endpoint ve auth pattern'leri için incelenebilir.

---

## Veri Modeli

### Entity'ler

| Entity | Alanlar (özet) |
|--------|----------------|
| **Tenant** | id, name, slug, plan (FREE/PRO/ENTERPRISE) |
| **User** | id, tenantId, email, role |
| **Restaurant** | id, tenantId, name, slug, logoUrl, templateId, colorPalette (JSON) |
| **TrendyolCredential** | tenantId, supplierId, apiKey (encrypted), apiSecret (encrypted), storeId, storeName, lastSyncAt |
| **Category** | id, restaurantId, externalId, name, sortOrder |
| **Product** | id, categoryId, externalId, name, description, sourcePrice, displayPrice, imageUrl, isAvailable |
| **ProductOption** | id, productId, name, price, isRequired, maxSelection |
| **ProductPriceOverride** | productId, displayPrice |
| **Table** | id, restaurantId, number, label |
| **MenuTheme** | restaurantId, templateId, colorPalette |
| **ServiceRequest** | id, restaurantId, tableNumber, type (WAITER/BILL), status (PENDING/ACKNOWLEDGED/COMPLETED), createdAt |
| **SyncLog** | id, tenantId, status, startedAt, completedAt, errorMessage |

### Multi-tenancy

Tüm tablolarda `tenantId`; EF Core global query filter ile otomatik filtre.

**Seed data (DbInitializer):** Uygulama ilk ayağa kalktığında aktif HTTP isteği olmadığı için `ITenantProvider` boş döner. Bu yüzden seed aşamasında:
1. Tüm `ITenantEntity` kayıtlarına manuel `TenantId` atanır (örn. `"lezzet-duragi"`).
2. Tenant kontrolü ve okuma işlemlerinde `IgnoreQueryFilters()` kullanılarak global filter aşılır.
3. `AppDbContext.ApplyChangeTracking`, `TenantId` zaten set edilmişse üzerine yazmaz.

### Fiyat stratejisi

- `sourcePrice` — Trendyol'dan gelen fiyat
- `displayPrice` — müşteriye gösterilen fiyat
- Override varsa `displayPrice` kullanılır

---

## URL ve QR Kod Yapısı

**Public menü URL'leri:**
- Genel: `https://menuapp.com/r/{slug}`
- Masa kodlu: `https://menuapp.com/r/{slug}?t=12` veya `/r/{slug}/t/12`

**QR kod:**
- Admin panelde restoran/masa bazlı QR üretimi (PNG indir)
- Masa numarası URL query param veya path segment olarak kodlanır
- İleride: baskıya hazır PDF export

---

## Menü Şablonları ve Renk Paleti

3 adet başlangıç şablonu (Blazor component):

| Şablon | Stil | Kullanım |
|--------|------|----------|
| `classic` | Liste görünümü, kategori sekmeleri | Kafe / restoran |
| `card` | Kart grid, büyük görseller | Fast food |
| `minimal` | Tipografi odaklı, az görsel | Fine dining |

**Renk paleti:** CSS variable tabanlı tema:
- `primary`, `secondary`, `background`, `text`, `accent`
- Admin'de hazır 8-10 palet + custom renk seçici
- Canlı önizleme

Şablonlar tenant-agnostic; sadece `MenuData + ThemeConfig` prop alır. `DigitalMenu.Core` içinde tanımlanır.

---

## Müşteri Fonksiyonları (MVP)

### Garson Çağır / Hesap İste
- Menü altında sabit FAB veya alt bar
- Masa numarası QR'dan otomatik okunur; yoksa kullanıcıdan istenir
- `POST /api/service-requests` → `{ type: 'WAITER' | 'BILL', tableNumber, restaurantId }`
- Rate limit: aynı masa için 60 sn cooldown

### Garson Paneli (Staff View)
- `/staff/{restaurantId}` — tablet görünümü
- SignalR ile anlık bildirim listesi
- Durum: `PENDING → ACKNOWLEDGED → COMPLETED`
- Sesli bildirim (opsiyonel)

### SignalR — Frontend Mantığı

1. Ekran açıldığında `/hubs/menu` adresine WebSocket bağlantısı açılır.
2. Bağlantı başarıyla kurulduğunda `JoinRestoranGroup("kebapci-ali")` metodu tetiklenir.
3. Ekran, `ReceiveWaiterCall` ve `ReceiveBillRequest` olaylarını dinlemeye başlar. Bir sinyal geldiğinde ekranda sesli bir uyarı çalar ve ilgili masanın rengi kırmızıya döner.

### Hesap Öde (Faz 3 — placeholder)
- DB'de `PaymentIntent` tablosu rezerve
- UI'da "Yakında" badge
- iyzico/Stripe entegrasyonu için hook noktaları

---

## İşletme Admin Paneli

1. **Onboarding** — İşletme adı, slug, logo yükleme
2. **Trendyol Bağlantısı** — API kimlik bilgileri, restoran seçimi, ilk sync
3. **Menü Düzenleme** — Kategori/ürün listesi, fiyat override, açıklama, görsel, ürün gizleme
4. **Görünüm** — Şablon seçimi, renk paleti, canlı önizleme
5. **Masalar** — Masa numarası listesi (1-50 veya custom), her masa için QR indir
6. **QR Kodları** — Genel menü QR + toplu masa QR export
7. **Bildirimler** — Garson/hesap istekleri geçmişi
8. **Ayarlar** — İşletme bilgileri, logo, çalışma saatleri (opsiyonel)

---

## Uygulama Fazları

### Faz 1 — MVP (4-6 hafta)
- [ ] Solution scaffold, EF Core schema, tenant auth
- [ ] Trendyol credential kayıt + menü sync (kategori, ürün, fiyat, opsiyon, görsel)
- [ ] 2 menü şablonu + renk paleti seçimi
- [ ] Public menü sayfası (mobil-first, responsive)
- [ ] QR kod üretimi (genel + masa numaralı)
- [ ] Garson çağır + hesap iste + SignalR staff paneli
- [ ] Temel admin: logo, işletme bilgisi, fiyat düzenleme

### Faz 2 — Tamamlayıcı (2-3 hafta)
- [ ] 3. şablon, custom renk, toplu QR PDF export
- [ ] Masa yönetimi UI
- [ ] Sync geçmişi, hata logları
- [ ] Ürün görsel upload (Trendyol'dan gelmeyenler için)
- [ ] i18n (TR varsayılan, EN opsiyonel)

### Faz 3 — SaaS Olgunlaştırma
- [ ] Abonelik planları (Free / Pro / Enterprise) — iyzico veya Stripe
- [ ] Çoklu şube (multi-restaurant per tenant)
- [ ] Hesap öde entegrasyonu
- [ ] Analitik (QR tarama, popüler ürünler)
- [ ] Custom domain (`{slug}.restoran.com`)

---

## Kritik Teknik Kararlar

1. **Trendyol sync idempotency:** `externalId` + `tenantId` unique constraint; upsert mantığı
2. **Fiyat stratejisi:** `sourcePrice` ve `displayPrice` ayrı kolonlar
3. **Görsel fallback:** Trendyol URL yoksa placeholder + admin upload
4. **Public menü cache:** Output cache / CDN — hızlı yükleme
5. **Rate limiting:** Public endpoint'lerde IP + masa bazlı limit (ASP.NET Core Rate Limiting middleware)

---

## Platform Değişikliği Özeti

| Eski (Plan) | Yeni (.NET) |
|-------------|-------------|
| Turborepo + pnpm monorepo | .NET Solution (.slnx) |
| Next.js 15 (App Router) | Blazor WebAssembly |
| NestJS | ASP.NET Core API |
| Prisma | EF Core |
| WebSocket (Socket.io) | SignalR |
| Clerk / NextAuth | ASP.NET Core Identity + JWT |
| qrcode (npm) | QRCoder |
| Vercel + Railway | Azure / IIS |

**Değişmeyen:** İş kuralları, veri modeli, Trendyol entegrasyonu, QR yapısı, menü şablonları, admin panel sayfaları, faz planı.

---

## İlk Sprint Görevleri

1. `DigitalMenu.Core` — domain entity'ler, DTO'lar, Trendyol client interface
2. `DigitalMenu.API` — EF Core DbContext, tenant/restaurant/menu/sync/service-request endpoint'leri
3. `DigitalMenu.Client` — public menü route (`/r/{slug}`), admin dashboard iskeleti
4. `docker-compose.yml` — PostgreSQL + Redis
5. Trendyol Go API client implementasyonu (ROPNG.Trendyol referans alınarak)
