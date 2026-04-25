# Vista.Core Yetkilendirme Dokümantasyonu

## 📋 **Vista.Core Yetkilendirme Tablosu**

### **🔐 Roller (BenutzerRolle Enum)**

| Rol | Açıklama | Yetki Seviyesi |
|-----|----------|----------------|
| **SuperAdmin** | Sistem yöneticisi - En yüksek yetki | ⭐⭐⭐⭐⭐ |
| **Admin** | Yönetici - Geniş yetkiler | ⭐⭐⭐⭐ |
| **Manager** | Yönetici - Orta seviye | ⭐⭐⭐ |
| **Standard** | Standart kullanıcı | ⭐⭐ |
| **NurLesen** | Sadece okuma yetkisi | ⭐ |

---

### **📊 Controller Bazlı Yetkilendirme Matrisi**

| Controller | Endpoint Türü | Gerekli Rol(ler) | Açıklama |
|------------|---------------|------------------|----------|
| **AuthController** | Tüm Endpoint'ler | ❌ Yok (Anonim) | Login, Register, Token yenileme |
| **BenutzerController** | Tüm Endpoint'ler | ✅ Admin, SuperAdmin, Manager | Kullanıcı yönetimi (CRUD) |
| **KundeController** | Tüm Endpoint'ler | ✅ Authorize (Tüm roller) | Müşteri yönetimi |
| **ProjektController** | Tüm Endpoint'ler | ✅ Authorize (Tüm roller) | Proje yönetimi |
| **TicketController** | Tüm Endpoint'ler | ✅ Authorize (Tüm roller) | Ticket/Destek sistemi |
| **AbonnementController** | GET (Liste/Detay) | ✅ Authorize (Tüm roller) | Abonelik görüntüleme |
| **AbonnementController** | POST/PUT/DELETE | ✅ SuperAdmin, Admin | Abonelik oluşturma/düzenleme |
| **ZahlungController** | GET (Liste/Detay) | ✅ Authorize (Tüm roller) | Ödeme görüntüleme |
| **ZahlungController** | POST/PUT/DELETE | ✅ SuperAdmin, Admin | Ödeme oluşturma/düzenleme |
| **BerichtController** | Tüm Endpoint'ler | ✅ Authorize (Tüm roller) | Rapor yönetimi |
| **DashboardController** | Tüm Endpoint'ler | ✅ Authorize (Tüm roller) | Dashboard istatistikleri |
| **ChatController** | Tüm Endpoint'ler | ✅ Authorize (Tüm roller) | Chat/Mesajlaşma |
| **AnsprechpartnerController** | Tüm Endpoint'ler | ✅ Authorize (Tüm roller) | İletişim kişileri yönetimi |
| **TicketNachrichtController** | Tüm Endpoint'ler | ✅ Authorize (Tüm roller) | Ticket mesajları |

---

### **🎯 Özel Yetkiler ve Kısıtlamalar**

| İşlem | Minimum Rol | Notlar |
|-------|-------------|--------|
| **Kullanıcı Oluşturma/Düzenleme/Silme** | Admin, SuperAdmin, Manager | BenutzerController - Tam yetki |
| **Abonelik Oluşturma** | Admin, SuperAdmin | Sadece yöneticiler plan oluşturabilir |
| **Ödeme Kaydı Oluşturma** | Admin, SuperAdmin | Sadece yöneticiler ödeme kaydedebilir |
| **Müşteri İşlemleri** | Tüm yetkili kullanıcılar | Herkes görüntüleyebilir/düzenleyebilir |
| **Proje İşlemleri** | Tüm yetkili kullanıcılar | Herkes proje yönetimi yapabilir |
| **Ticket İşlemleri** | Tüm yetkili kullanıcılar | Herkes ticket açabilir/yönetebilir |
| **Dashboard Görüntüleme** | Tüm yetkili kullanıcılar | Herkes istatistikleri görebilir |
| **Chat/Mesajlaşma** | Tüm yetkili kullanıcılar | Herkes chat yapabilir |

---

### **📌 Önemli Notlar:**

1. **`[Authorize]`** = Tüm giriş yapmış kullanıcılar (tüm roller)
2. **`[Authorize(Roles = "Admin,SuperAdmin,Manager")]`** = Sadece Admin, SuperAdmin veya Manager
3. **Anonim Erişim** = Sadece AuthController (Login/Register)
4. **MandantId Kontrolü** = X-Mandant-Id header kontrolü birçok endpoint'te var (multi-tenant yapı)

---

### **🔍 Yetki Özeti:**

- **SuperAdmin, Admin & Manager**: Sistem yönetimi + kullanıcı yönetimi + tüm işlemler
- **Standard**: Normal kullanıcı işlemleri (müşteri, proje, ticket vb.) - Kullanıcı yönetimi YOK
- **NurLesen**: Sadece görüntüleme yetkisi - Hiçbir düzenleme/silme/ekleme yapamaz

---

**Oluşturulma Tarihi:** 2025
**Proje:** Vista.Core - SaaS Backend
**Teknoloji:** .NET 9/10 + ASP.NET Core Identity
