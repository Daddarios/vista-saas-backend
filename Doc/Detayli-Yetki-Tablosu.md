# Vista.Core - Detaylı Yetki Tablosu

## 📊 Sayfa ve İşlem Bazlı Yetkilendirme Matrisi

### **Tam Yetki Tablosu**

| Sayfa / İşlem | SuperAdmin | Admin | Manager | Standard | NurLesen |
|---------------|------------|-------|---------|----------|----------|
| **Benutzer sayfası erişimi** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Benutzer — Yeni/Düzenle/Sil** | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Kunden — Görüntüle** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Kunden — Yeni/Düzenle/Sil** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Projekte — Görüntüle** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Projekte — Yeni/Düzenle/Sil** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Tickets — Görüntüle/Detay** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Tickets — Yeni/Düzenle/Sil** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Abonnements — Görüntüle** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Abonnements — Yeni/Düzenle/Sil** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Zahlungen — Görüntüle** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Zahlungen — Yeni/Düzenle/Sil** | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Berichte — İndir** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Berichte — Yükle/Sil** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Dashboard** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Chat** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Ansprechpartner — Görüntüle** | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Ansprechpartner — Yeni/Düzenle/Sil** | ✅ | ✅ | ✅ | ✅ | ❌ |
| **Sidebar Benutzer menüsü** | ✅ | ✅ | ✅ | ❌ | ❌ |

---

## 🔍 Rol Açıklamaları

### **SuperAdmin** ⭐⭐⭐⭐⭐
- Sistemdeki **en yüksek yetki**
- Her şeyi yapabilir
- Hiçbir kısıtlama yok

### **Admin** ⭐⭐⭐⭐
- Yönetici seviyesinde geniş yetkiler
- Kullanıcı yönetimi yapabilir
- Abonelik ve ödeme yönetimi yapabilir
- Sistem yönetimi için gerekli tüm yetkiler

### **Manager** ⭐⭐⭐
- **YENİ:** Artık kullanıcı yönetimi yapabilir
- Normal operasyonel işlemleri yapabilir
- Müşteri, proje, ticket yönetimi
- Abonelik ve ödeme yönetimi YAPAMAZ

### **Standard** ⭐⭐
- Standart kullanıcı
- Günlük operasyonel işlemleri yapabilir
- Kullanıcı yönetimi YAPAMAZ
- Abonelik ve ödeme yönetimi YAPAMAZ

### **NurLesen** ⭐
- **Sadece okuma yetkisi**
- Hiçbir ekleme/düzenleme/silme işlemi yapamaz
- Tüm sayfaları görüntüleyebilir (Benutzer hariç)
- İstatistiklere ve raporlara erişebilir

---

## 📌 Önemli Notlar

1. **Manager Rolü Güncellemesi:**
   - ✅ Artık `/benutzer` sayfasına erişebilir
   - ✅ Kullanıcı ekleme/düzenleme/silme yapabilir
   - ✅ Sidebar'da "Benutzer" menüsünü görebilir

2. **Finansal İşlemler:**
   - ❌ Manager, Standard ve NurLesen rolleri abonelik ve ödeme yönetimi **YAPAMAZ**
   - ✅ Sadece SuperAdmin ve Admin finansal işlem yapabilir

3. **NurLesen Özel Durumu:**
   - ❌ Hiçbir CRUD işlemi yapamaz
   - ✅ Tüm sayfalarda sadece görüntüleme yetkisi var
   - ❌ "Ekle", "Düzenle", "Sil" butonlarını göremez

---

**Son Güncelleme:** 2025  
**Güncelleme Nedeni:** Manager rolüne kullanıcı yönetimi yetkisi eklendi  
**Etkilenen Sayfalar:** BenutzerController, Sidebar Menü, Frontend Route Korumaları
