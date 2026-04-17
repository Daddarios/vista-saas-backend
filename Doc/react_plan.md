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
