# Dynamic Configuration System

.NET 8 ile yazılmış, MongoDB storage, in-memory cache ve otomatik arka plan yenileme desteği sunan dinamik konfigürasyon yönetim sistemi.

## Hızlı Başlangıç

**Tek gereklilik: Docker Desktop**

```bash
git clone https://github.com/umuterdal/dynamic-configuration-system.git
cd dynamic-configuration-system
docker-compose up -d
```

| Servis | URL |
|--------|-----|
| Admin Panel | http://localhost:5000 |
| Demo API (Swagger) | http://localhost:5001/swagger |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

Durdurmak için:
```bash
docker-compose down
```

## Ne Yapar?

Uygulama restart olmadan konfigürasyon değişikliklerini otomatik olarak algılayan, tip güvenli, cache'li, service-isolated bir konfigürasyon yönetim sistemi.

- **Dinamik Konfigürasyon**: Deployment veya restart olmadan runtime güncellemeler
- **Service Isolation**: Her servis sadece kendi konfigürasyon kayıtlarını görür
- **Tip Güvenliği**: `GetValue<T>()` ile generic, strongly-typed erişim
- **Thread Güvenliği**: ConcurrentDictionary ile eşzamanlı erişim
- **Otomatik Yenileme**: BackgroundService + PeriodicTimer (30sn)
- **Graceful Degradation**: Storage erişilemezse son başarılı cache ile çalışmaya devam
- **Admin Panel**: CRUD işlemleri için web arayüzü
- **Message Broker**: RabbitMQ ile anlık cache yenileme (polling'i tamamlar)

## ConfigurationReader Kullanımı

```csharp
// 3 parametre ile initialize
var reader = new ConfigurationReader(
    applicationName: "SERVICE-A",
    connectionString: "mongodb://localhost:27017",
    refreshTimerIntervalInMs: 30000);

// Değer okuma
string siteName = reader.GetValue<string>("SiteName");      // "soty.io"
int maxItems = reader.GetValue<int>("MaxItemCount");        // 500
double taxRate = reader.GetValue<double>("TaxRate");        // 0.18
bool isEnabled = reader.GetValue<bool>("IsBasketEnabled"); // true

// Güvenli erişim
if (reader.TryGetValue<string>("SiteName", out var name))
    Console.WriteLine(name);

// Tüm değerleri getir
var allValues = reader.GetAllValues();

// Manuel yenileme
await reader.RefreshAsync();
```

## API Endpoints (Demo API)

| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/demo/value/{key}` | String değer getir |
| GET | `/api/demo/typed/{key}?type=int` | Tip belirterek değer getir |
| GET | `/api/demo/all` | Tüm konfigürasyonları getir |
| POST | `/api/demo/refresh` | Cache'i yenile |

## Mimari

```
Configuration.Domain        → Entity'ler, interface'ler (dış bağımlılık yok)
Configuration.Application   → İş mantığı, RabbitMQ publisher
Configuration.Infrastructure → MongoDB repository
Configuration.Library       → ConfigurationReader (dışarıya açık API)
Configuration.Admin         → ASP.NET Core MVC Admin Panel
Configuration.DemoApi       → Swagger'lı Demo API
```

## Ekstra Puan Kriterleri

| Kriter | Durum |
|--------|-------|
| Message Broker (RabbitMQ) | ✅ |
| TPL, async/await | ✅ |
| Concurrency handling | ✅ ConcurrentDictionary, Interlocked |
| Design Patterns | ✅ Clean Architecture, SOLID, Repository, DI |
| TDD | ✅ |
| Unit Tests | ✅ 39 test |
| MongoDB | ✅ |
| Çalışır Proje | ✅ docker-compose up -d |
| Dokümantasyon | ✅ README.md |
| Source Control | ✅ GitHub |
| Docker Compose | ✅ 4 container |
