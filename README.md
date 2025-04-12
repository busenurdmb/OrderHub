# 📦 OrderHub - Dağıtık Sipariş Yönetim Sistemi

OrderHub, .NET 9 ile geliştirilmiş, mikroservis mimarisine sahip, mesaj tabanlı (event-driven) ve tutarlılık odaklı bir sipariş yönetim sistemidir. Redis, RabbitMQ, Elasticsearch, Serilog gibi modern araçlarla donatılmıştır.

---

## 🧠 Bu Projede Öğrenilenler

- ⚙️ Mikroservis mimarisi
- 📬 RabbitMQ ile servisler arası iletişim
- 📤 Outbox Pattern ile mesaj kayıplarının önlenmesi
- 🔄 Saga Pattern ile dağıtık transaction yönetimi
- 🧠 Redis ile önbellekleme
- 📊 Elasticsearch + Kibana ile log görselleştirme
- 📑 PostgreSQL + Dapper ile veri yönetimi

---

## 📚 Kullanılan Teknolojiler

| Teknoloji         | Açıklama                                              |
|------------------|-------------------------------------------------------|
| ✅ .NET 9         | API, Worker Service ve tüm uygulama iskeleti         |
| 🐇 RabbitMQ       | Event tabanlı servis haberleşmesi                    |
| 🧾 Serilog        | Loglama altyapısı (Elasticsearch)                    |
| 🔍 Elasticsearch  | Logların indekslenmesi                               |
| 📊 Kibana         | Logların analiz edilmesi                             |
| 📦 PostgreSQL     | Tüm servislerin veri saklama altyapısı               |
| 🧠 Redis          | Sipariş ve stok için önbellek sistemi                 |
| 🧪 Dapper         | Minimal ve hızlı veri erişimi                         |
| 🧽 Saga Pattern   | Dağıtık transaction yönetimi (stock & payment flow) |
| 📤 Outbox Pattern | Veri + Event senkronizasyonu                         |
| 🐳 Docker Compose | RabbitMQ, Postgres, Elastic stack containerları      |

---

## 🧩 Saga Pattern Nedir?

Saga Pattern, dağıtık sistemlerde birden fazla mikroservisin parça parça bir işi tamamlamasını sağlar. Her adım bir event fırlatır ve sıradaki servis bu eventi yakalar.

**Bizim projede:**

1. `Order.API` → siparişi oluşturur
2. `Stock.API` → ürünü rezerve eder
3. `Payment.API` → ödeme işlemini gerçekleştirir

Eğer bir adım başarısız olursa önceki adımların rollback edilmesini sağlayan **Choreography tabanlı Saga** uygulanmıştır.

---
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/rabbitmqexchange.png)
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/saga.png)
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/rabbit1.png)
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/rabbit12.png)

## 🧭 Exchange ile Başlangıç – Fanout Modeli

İlk olarak mikroservisler arası iletişimde **RabbitMQ Exchange** yapısını kullandık. Burada:

- `OrderCreatedEvent` → `order-created-exchange` isimli bir **Fanout Exchange** üzerinden yayınlandı.
- Hem `Stock.API` hem de `Payment.API`, bu exchange'e bağlanan ayrı kuyruklarla mesajları tüketti.
- Yani **bir event → birden fazla servis** tarafından eş zamanlı işlendi.

**Avantajı:**  
✔ Kolay kurulum, basit yapı  
✖ Ancak ödeme servisi, stok işleminden önce çalışabiliyor — bu da **veri tutarsızlığına** yol açabiliyor.

---

## 🔄 Saga Pattern’a Geçiş – İş Akışında Tutarlılık

İlk yapıda `OrderCreatedEvent`, hem stok hem ödeme servisine eşzamanlı gidiyordu. Bu durum şu sorunlara neden olabiliyordu:

- 📉 **Stok yetersizse bile ödeme alınabiliyordu**
- 🤯 Event’ler kontrolsüz şekilde işleniyordu

### Bu nedenle yapı **SAGA Pattern** ile yeniden tasarlandı:

| Aşama | Açıklama |
|-------|----------|
| 1️⃣   | `Order.API`, siparişi veritabanına kaydeder ve `OrderCreatedEvent` fırlatır |
| 2️⃣   | `Stock.API` sadece bu eventi dinler ve stok kontrolü yapar |
| 3️⃣   | Eğer stok yeterliyse: `StockReservedEvent` fırlatılır |
| 4️⃣   | `Payment.API` sadece bu **StockReservedEvent**’i dinler ve ödeme işlemini başlatır |
| 5️⃣   | Ödeme başarılıysa süreç tamamlanır |

> ❌ Eğer stok yetersizse `StockFailedEvent` fırlatılır ve süreç durdurulur.

---

### 🎯 Sonuç:

SAGA Pattern sayesinde:

- ✔ Servisler **senkron bir sıra ile** çalışır
- ✔ Her adımın başarısı bir sonraki adımı tetikler
- ✔ Hatalı durumda rollback ya da event fırlatılması mümkündür
- ✔ Daha **tutarlı, güvenli ve izlenebilir** bir yapı kurulmuş olur

---

### 🧩 Fanout vs Saga (Karşılaştırma)

| Özellik                  | Fanout Exchange Yapısı | Saga Pattern |
|--------------------------|------------------------|--------------|
| Adım sırası kontrolü     | ❌ Yok                 | ✅ Var       |
| Servis bağımlılığı       | ❌ Yok                 | ✅ Var       |
| Tutarlılık               | ❌ Riskli              | ✅ Güçlü     |
| Yönetilebilirlik         | ✅ Basit               | ✅ Orta      |
| Hata senaryosu yönetimi  | ❌ Yok                 | ✅ Var       |

---


---

## 📤 Outbox Pattern Nedir?

Outbox Pattern, veritabanına yazılan verinin kaybolmadan mesaj sistemine (RabbitMQ) iletilmesini garanti altına alır.

- Sipariş alındığında veritabanına kaydedilir.
- Aynı anda `Outbox` tablosuna event düşer.
- `OutboxPublisherWorker` bu tabloyu dinler ve RabbitMQ’ya event gönderir.
- Böylece event, veritabanıyla senkronize olarak gider.

---

## 🧱 Proje Katmanları

```
OrderHub/
├── Order.API             → Sipariş oluşturma REST API'si
├── Stock.API             → Stok rezervasyon servisi
├── Payment.API           → Ödeme işleme servisi
├── Shared/               → Ortak yapılar (Events, DTOs)
├── Infrastructure/       → Serilog, Elastic, RabbitMQ, Redis
├── Worker/               → OutboxPublisher (BackgroundService)
└── docker-compose.yml    → PostgreSQL, RabbitMQ, Elasticsearch servis tanımı
```

---

![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/Dashboard.png)
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/elastick.png)
## 📦 Sipariş Akışı

```
Kullanıcı → Order.API → OutboxMessage → RabbitMQ → Stock.API
            ↓
        Elasticsearch Log
```

Devamı:
```
Stock.API → Stock kontrol
         → Başarılıysa: StockReservedEvent → RabbitMQ → Payment.API
         → Başarısızsa: StockFailedEvent (rollback)
```

---

## 🔀 Outbox Publisher Worker

- Arka planda çalışır
- `OutboxMessages` tablosundaki mesajları okur
- RabbitMQ’ya gönderir
- Başarılı gönderilen mesajlar silinir veya işaretlenir

---
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/posgres.png)
## 📦 Örnek OutboxMessage Tablosu

| Id | Type              | Content                                 | CreatedOnUtc        |
|----|-------------------|------------------------------------------|---------------------|
| 1  | OrderCreatedEvent | { "OrderId": "...", "Items": [...] }     | 2025-04-12T12:00:00 |

---

![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/POST.png)
## 🧾 Örnek Event Akışı

```json
{
  "OrderId": "f4c2d...",
  "CustomerName": "Buse Nur Demirbaş",
  "TotalPrice": 370.00,
  "CreatedAt": "2025-04-12T12:30:00",
  "Items": [
    {
      "ProductId": "abc123",
      "Quantity": 2
    }
  ]
}
```

---
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/Screenshot%202025-04-11%20134020.png)
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/Docker.png)

## 🐳 Docker Compose

```yaml
version: "3.7"

services:
  postgres:
    image: postgres:15
    container_name: orderhub_postgres
    restart: always
    environment:
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: password
      POSTGRES_DB: orderhub
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  redis:
    image: redis:7
    container_name: orderhub_redis
    ports:
      - "6381:6379"

  rabbitmq:
    image: rabbitmq:3-management
    container_name: orderhub_rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: password
    ports:
      - "5674:5672" # uygulama bağlantısı
      - "15674:15672" # yönetim paneli

  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:8.13.0
    container_name: orderhub_elasticsearch
    environment:
      - discovery.type=single-node
      - xpack.security.enabled=false
      - ES_JAVA_OPTS=-Xms512m -Xmx512m
    ports:
      - "9200:9200"
    volumes:
      - esdata:/usr/share/elasticsearch/data
    networks:
      - app-net

  kibana:
    image: docker.elastic.co/kibana/kibana:7.17.0
    container_name: orderhub_kibana
    ports:
      - "5601:5601"
    environment:
      - ELASTICSEARCH_HOSTS=http://elasticsearch:9200
    depends_on:
      - elasticsearch
    networks:
      - app-net

volumes:
  pgdata:
  esdata:
    driver: local

networks:
  app-net:
    driver: bridge
```

---
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/Dashboard.png)
![Kibana index](https://github.com/busenurdmb/OrderHub/blob/master/images/elastick.png)
## 📥 Kurulum

```bash
git clone https://github.com/busenurdmb/OrderHub.git
cd OrderHub
docker-compose up -d
```

---

## 👩‍💻 Katkı ve Lisans

> Hazırlayan: [@busenurdmb](https://github.com/busenurdmb)  
> Lisans: MIT  
> Bu proje açık kaynak olup, katkılarınızı memnuniyetle bekler!
