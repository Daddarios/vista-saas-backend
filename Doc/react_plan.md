# CRM SaaS — React Frontend Master Plan

> **KULLANIM:** Bu plan, Backend API (`crmsaas_plan.md`) ile entegre çalışacak modern bir SPA (Single Page Application) için adım adım geliştirme rehberidir.

---

## 🚀 Tech Stack

| Katman | Seçim | Neden? |
|--------|-------|--------|
| **Core** | Vite + React (Vanilla JS) | Süper hızlı tam donanımlı derleyici, TS/Next.js olmadan sade yapı |
| **Routing** | React Router v6 | SPA içi sayfa geçişleri ve `ProtectedRoute` izolasyonu |
| **Styling** | Bootstrap 5 + Vanilla CSS | Hızlı bileşen geliştirme ve portfolyo estetiği |
| **Network** | Axios | Merkezi Interceptor (Token ekleme, 401 hatası yakalama) |
| **State** | Context API | Login durumu ve kullanıcı bilgisini globalde tutmak için |
| **Real-time**| `@microsoft/signalr` | Canlı chat ve Ticket son dakika bildirimleri |

---

## 📂 Klasör Mimarisi

Büyük projelerde spagetti kodu engellemek için **özellik (Feature) tabanlı** değil, **tür (Type) tabanlı** basit yapı tercih edilmiştir.

```text
frontend/
├── src/
│   ├── api/                  # Backend'e giden tüm istekler
│   │   ├── axiosClient.js    # Interceptor'lı merkezi axios instance
│   │   ├── authApi.js        # Login, Verify
│   │   ├── kundeApi.js       # Müşteri CRUD vb.
│   │
│   ├── assets/               # Statik imajlar / css kural setleri
│   │   ├── css/
│   │   │   └── global.css    # Custom override'lar
│   │
│   ├── components/           # Tekrar kullanılabilir yapılar
│   │   ├── Layout/           # Sidebar, Navbar, Footer
│   │   ├── Shared/           # Custom Input, Buton, Spinner, DataTable
│   │   ├── Chat/             # Chat formları ve mesaj listesi
│   │
│   ├── context/              # Global state yöneticisi
│   │   ├── AuthContext.jsx   # user verisi ve isLoggedIn statüleri
│   │
│   ├── hooks/                # Custom hook'lar
│   │   ├── useSignalR.js     # Chat ve bildirim bağlantısını yönetir
│   │
│   ├── pages/                # Route'larda gösterilecek ana sayfa bileşenleri
│   │   ├── Login.jsx
│   │   ├── Dashboard.jsx
│   │   ├── Kunden/
│   │   │   ├── KundenList.jsx
│   │   │   └── KundeDetail.jsx
│   │   ├── Tickets/
│   │   └── Chat/
│   │
│   ├── App.jsx               # Router tanımları (Browser Router)
│   ├── main.jsx              # Uygulama başlangıcı (Context sarmalayıcıları)
│
├── .env                      # VITE_API_BASE_URL (http://localhost:5000)
└── package.json
```

---

## 🔐 Authentication ve Axios Interceptor Akışı

Mimaride token yönetimi en kritik noktadır. Tüm token süreci tek bir dosyada (`axiosClient.js`) halledilir; sayfalar token işleriyle uğraşmaz.

```javascript
// axiosClient.js
import axios from 'axios';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL,
    withCredentials: true // Cookie bazlı token için MÜHİM!
});

// Request Interceptor: Eğer localstorage veya cookie dışında eklenecek bir şey varsa.
api.interceptors.request.use(config => {
    // Cookie kullanıldığı için Bearer setlemeye gerek kalmayabilir
    // ancak JWT header'da ise:
    const token = localStorage.getItem('token');
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
});

// Response Interceptor: 401 hatası (Token expired) alırsa Login'e fırlat
api.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 401) {
            window.location.href = '/login'; 
        }
        return Promise.reject(error);
    }
);

export default api;
```

---

## 🧭 Routing & Protected Route

```javascript
// App.jsx Örneği
function App() {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<Login />} />
        
        {/* SADECE GİRİŞ YAPANLAR */}
        <Route element={<ProtectedRoute />}>
           <Route element={<Layout />}>
             <Route path="/" element={<Dashboard />} />
             <Route path="/kunden" element={<KundenList />} />
             <Route path="/tickets" element={<TicketsList />} />
           </Route>
        </Route>
      </Routes>
    </Router>
  );
}
```

---

## 🗺️ FRONTEND UYGULAMA FAZLARI (FAZ 8 Detayı)

`crmsaas_plan.md` içerisindeki **FAZ 8**, kendi içinde aşağıdaki adımlardan oluşur. Başlandığında bu adımlar sırayla tamamlanacak.

- [ ] **F.1** `npm create vite@latest frontend -- --template react` ile proje oluştur
- [ ] **F.2** Bootstrap 5, Axios, React Router Dom paketlerini yükle
- [ ] **F.3** `.env` dosyası ve `axiosClient.js` (Interceptor) kurulumunu yap
- [ ] **F.4** `AuthContext.jsx` ve `<ProtectedRoute />` yapısını hazırla
- [ ] **F.5** Sayfa İskeleti: `Sidebar` ve `Navbar` (Layout) yapısını Bootstrap ile tasarla
- [ ] **F.6** `Login` Sayfası + Backend POST isteği ile bağlantı
- [ ] **F.7** `Dashboard` Sayfası (Kutu istatistikleri ve grafik taslakları)
- [ ] **F.8** `KundenList.jsx` ve CRUD Modal yapıları (Arama ve Tablo)
- [ ] **F.9** `Ticket` Sayfası ve Canlı bildirim bağlantısı (`SignalR` Hook çalışması)
- [ ] **F.10** `Chat` Sayfası UI ve Canlı mesaj iletimi testleri

---

> **Tasarım Notu:** Portfolyo vitrini olduğu için Bootstrap CSS standart bırakılmayacak; bolca mikro animasyon, temiz buton yuvarlamaları (border-radius), yumuşak bir renk paleti ve modern gölgeler (box-shadow) kullanılacaktır. Çalışırken CSS estetiği 1. plandadır.

---

## 🔗 Backend API Endpoint Tablosu

> Frontend'den backend'e yapılacak tüm isteklerin tam listesi.
> Her istekte `X-Mandant-Id` header'ı (Tenant GUID) gönderilmelidir.
> Auth cookie (httpOnly JWT) otomatik gider — `withCredentials: true` yeterli.

### 🔐 Auth

| Metod | Endpoint | Body / Params | Açıklama |
|-------|----------|---------------|----------|
| POST | `/api/auth/login` | `{ email, passwort }` | Giriş → 2FA kod gönderir |
| POST | `/api/auth/verify` | `{ email, code }` | 2FA doğrula → JWT cookie set eder |
| POST | `/api/auth/refresh` | — | Token yenile |
| POST | `/api/auth/logout` | — | Çıkış |

### 👥 Kunde (Müşteri)

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/kunde?page=1&size=20&search=` | Query params |
| GET | `/api/kunde/{id}` | — |
| POST | `/api/kunde` | `{ unternehmen, vorname, nachname, email, telefonMobil, telefonHaus, adresse, website, hinweise }` |
| PUT | `/api/kunde/{id}` | Aynı body |
| DELETE | `/api/kunde/{id}` | — |

### 👤 Ansprechpartner (İletişim Kişisi)

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/ansprechpartner?kundeId=` | Kunde bazlı listeleme |
| POST | `/api/ansprechpartner` | `{ name, telefon, email, abteilung, kundeId }` |
| PUT | `/api/ansprechpartner/{id}` | Aynı body |
| DELETE | `/api/ansprechpartner/{id}` | — |

### 📁 Projekt

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/projekt?page=1&size=20&search=` | Query params |
| GET | `/api/projekt/{id}` | — |
| POST | `/api/projekt` | `{ name, beschreibung, startdatum, enddatum, status, prioritaet, abschlussInProzent }` |
| PUT | `/api/projekt/{id}` | Aynı body |
| DELETE | `/api/projekt/{id}` | — |

### 🎫 Ticket

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/ticket?page=1&size=20&search=&status=` | Query params |
| GET | `/api/ticket/{id}` | — |
| POST | `/api/ticket` | `{ titel, beschreibung, status, prioritaet, kategorie, faelligkeitsdatum, kundeId, projektId }` |
| PUT | `/api/ticket/{id}` | Aynı body |
| DELETE | `/api/ticket/{id}` | — |
| PATCH | `/api/ticket/{id}/status?status=Geloest` | Query param |

### 💬 Ticket Nachrichten (Mesajlar)

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/ticketnachricht/ticket/{ticketId}` | — |
| POST | `/api/ticketnachricht` | `{ ticketId, inhalt, istInternNotiz }` |

### 💬 Chat

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/chat/raeume` | — |
| GET | `/api/chat/raum/{raumId}/nachrichten?page=1&size=50` | Query params |

### 📊 Dashboard

| Metod | Endpoint | Açıklama |
|-------|----------|----------|
| GET | `/api/dashboard` | Tüm istatistikler (kundenAnzahl, projekteAnzahl, ticketsAnzahl, offeneTickets...) |

### 👤 Benutzer (Kullanıcı Yönetimi)

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/benutzer` | Tüm kullanıcılar |
| GET | `/api/benutzer/{id}` | — |
| POST | `/api/benutzer` | `{ vorname, nachname, email, rolle }` |
| PUT | `/api/benutzer/{id}` | Aynı body |
| DELETE | `/api/benutzer/{id}` | — |

### 📄 Bericht (Rapor/Dosya)

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/bericht` | Liste |
| POST | `/api/bericht` | `multipart/form-data` (dosya upload) |
| GET | `/api/bericht/{id}/download` | Dosya indir |
| DELETE | `/api/bericht/{id}` | — |

### 💳 Abonnement (Taslak)

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/abonnement` | Liste |
| GET | `/api/abonnement/{id}` | — |
| GET | `/api/abonnement/plaene` | Sabit plan listesi (AllowAnonymous) |
| POST | `/api/abonnement` | `{ plan, planName, preis, startDatum, endDatum }` |
| PUT | `/api/abonnement/{id}` | Aynı body |
| DELETE | `/api/abonnement/{id}` | — |

### 💰 Zahlung (Taslak)

| Metod | Endpoint | Body / Params |
|-------|----------|---------------|
| GET | `/api/zahlung` | Liste |
| GET | `/api/zahlung/{id}` | — |
| POST | `/api/zahlung` | `{ rechnungId, betrag, iban, hinweise }` |
| PATCH | `/api/zahlung/{id}/status?status=Abgeschlossen` | Query param |

### 🔌 SignalR Hub Bağlantıları

| Hub | URL | Metodlar |
|-----|-----|----------|
| Chat | `/hubs/chat` | `JoinRoom(raumId)`, `SendMessage(raumId, inhalt)` → dinle: `ReceiveMessage`, `UserTyping` |
| Bildirim | `/hubs/benachrichtigung` | dinle: `TicketUpdated`, `NewNotification` |

### ⚙️ Her İstekte Gerekli Header

| Header | Değer | Açıklama |
|--------|-------|----------|
| `X-Mandant-Id` | `Guid` | Tenant izolasyonu |
| Cookie | httpOnly JWT | Otomatik — `withCredentials: true` |
