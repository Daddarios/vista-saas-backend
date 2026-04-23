# 📸 Vista.Core - Dosya Yükleme Sistemi

## 🎯 Özellikler

✅ **Müşteri (Kunde) Logo Yükleme**  
✅ **Kullanıcı (Benutzer) Avatar Yükleme**  
✅ **PNG, JPEG, JPG Desteği** (Max 5MB)  
✅ **Güvenli Dosya Yönetimi** (Path traversal koruması)  
✅ **Otomatik Dosya Silme** (Entity silindiğinde)  
✅ **Null-Safe** (Resim boş kalabilir, sorun çıkmaz)  
✅ **Eski Dosya Temizleme** (Yeni yüklemede eski otomatik silinir)

---

## 📁 Klasör Yapısı

```
Vista.Core/
├── Storage/
│   ├── Logos/         # Müşteri logoları
│   │   └── {kundeId}-{timestamp}.png
│   └── Avatars/       # Kullanıcı avatarları
│       └── {benutzerId}-{timestamp}.jpg
```

**Not:** `Storage` klasörü otomatik oluşturulur, manuel oluşturmaya gerek yok!

---

## 🚀 API Kullanımı

### 1️⃣ Müşteri Logo İşlemleri

#### **Logo Yükle**
```http
POST /api/kunde/{kundeId}/upload-logo
Content-Type: multipart/form-data

Body:
  logoFile: [dosya seç: .png, .jpg, .jpeg - Max 5MB]
```

**Başarılı Response:**
```json
{
  "logo": "/storage/Logos/abc123-1672531200.png",
  "nachricht": "Logo başarıyla yüklendi."
}
```

**Hata Response:**
```json
{
  "nachricht": "Dosya boyutu çok büyük. Max: 5MB"
}
```

#### **Logo Sil**
```http
DELETE /api/kunde/{kundeId}/delete-logo
```

**Başarılı Response:**
```json
{
  "nachricht": "Logo başarıyla silindi."
}
```

---

### 2️⃣ Kullanıcı Avatar İşlemleri

#### **Avatar Yükle**
```http
POST /api/benutzer/{benutzerId}/upload-avatar
Content-Type: multipart/form-data

Body:
  avatarFile: [dosya seç: .png, .jpg, .jpeg - Max 5MB]
```

**Başarılı Response:**
```json
{
  "avatar": "/storage/Avatars/user456-1672531200.jpg",
  "nachricht": "Avatar başarıyla yüklendi."
}
```

#### **Avatar Sil**
```http
DELETE /api/benutzer/{benutzerId}/delete-avatar
```

---

## 🖼️ Frontend Entegrasyonu (React)

### **Müşteri Logo Yükleme**

```jsx
const uploadLogo = async (kundeId, file) => {
  const formData = new FormData();
  formData.append('logoFile', file);

  const response = await fetch(`/api/kunde/${kundeId}/upload-logo`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  const data = await response.json();
  console.log('Yüklendi:', data.logo);
  // data.logo = "/storage/Logos/abc123-1672531200.png"
};

// Kullanım:
<input 
  type="file" 
  accept=".png,.jpg,.jpeg" 
  onChange={(e) => uploadLogo(kundeId, e.target.files[0])} 
/>

// Gösterim:
<img src={`${API_URL}${kunde.logo}`} alt="Logo" />
// Örnek: http://localhost:5000/storage/Logos/abc123-1672531200.png
```

### **Kullanıcı Avatar Yükleme**

```jsx
const uploadAvatar = async (benutzerId, file) => {
  const formData = new FormData();
  formData.append('avatarFile', file);

  const response = await fetch(`/api/benutzer/${benutzerId}/upload-avatar`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  const data = await response.json();
  console.log('Yüklendi:', data.avatar);
};
```

---

## 🔧 Backend Detayları

### **FileStorageService**

```csharp
// Kullanım:
private readonly FileStorageService _fileStorage;

// Upload
var (success, fileUrl, errorMessage) = await _fileStorage.UploadFileAsync(
    file: logoFile,
    folderName: FileStorageService.LogosFolder,
    uniqueId: kundeId.ToString(),
    oldFilePath: kunde.Logo // Eski dosyayı otomatik sil
);

// Delete (null-safe)
await _fileStorage.DeleteFileAsync(kunde.Logo); // Logo null olsa bile hata vermez

// Kontrol
bool exists = _fileStorage.FileExists(kunde.Logo);
```

---

## 🛡️ Güvenlik Özellikleri

| Özellik | Açıklama |
|---------|----------|
| **Format Validation** | Sadece PNG, JPEG, JPG kabul edilir |
| **Size Limit** | Max 5MB dosya boyutu |
| **Path Traversal Protection** | `../../../` gibi saldırılara karşı koruma |
| **Unique Filenames** | `{id}-{timestamp}.ext` formatı (collision yok) |
| **Null Safety** | Resim yoksa bile hata vermez |
| **Cascade Delete** | Entity silinince dosya da silinir |

---

## 📋 Önemli Notlar

### ⚠️ Dikkat Edilmesi Gerekenler:

1. **Resim Opsiyonel:**
   - `kunde.Logo` ve `benutzer.Bild` boş kalabilir
   - Null kontrolü yapılıyor, hata vermez

2. **Eski Dosya Otomatik Silinir:**
   - Yeni logo yüklendiğinde eski otomatik silinir
   - Elle silmeye gerek yok

3. **Soft Delete Entegrasyonu:**
   - `Kunde` soft delete edildiğinde logo dosyası da silinir
   - `Benutzer` hard delete edildiğinde avatar dosyası da silinir

4. **Production Deployment:**
   - `/Storage` klasörü `.gitignore`'da olmalı
   - Cloud deployment'ta persistent volume kullan (Azure Blob, AWS S3 vs.)

---

## 🧪 Test

```bash
# Build
dotnet build

# Test
dotnet test

# Çalıştır
dotnet run --project Vista.Core
```

### **Manuel Test (Postman/Thunder Client):**

1. `POST /api/auth/login` → Token al
2. `POST /api/kunde` → Müşteri oluştur
3. `POST /api/kunde/{id}/upload-logo` → Logo yükle
   - Headers: `Authorization: Bearer {token}`
   - Body: `form-data`, key: `logoFile`, value: dosya seç
4. `GET /api/kunde/{id}` → Logo URL'ini gör
5. Tarayıcıda aç: `http://localhost:5000/storage/Logos/{filename}`

---

## 🎨 Docker Deployment

Dockerfile'da Storage klasörü için volume:

```dockerfile
# Storage klasörünü oluştur
RUN mkdir -p /app/Storage/Logos /app/Storage/Avatars

# Volume mount (opsiyonel)
VOLUME ["/app/Storage"]
```

Docker Compose:

```yaml
services:
  vista-api:
    build: .
    volumes:
      - ./storage:/app/Storage  # Host ile senkronize
    ports:
      - "8080:8080"
```

---

## 🚀 Railway/Neon Deployment

⚠️ **Önemli:** Railway gibi container platformlarda dosyalar **geçici**dir!

**Çözüm Seçenekleri:**

1. **Azure Blob Storage** (önerilen)
2. **AWS S3**
3. **Cloudinary**
4. **Railway Persistent Volumes** (ücretli)

---

## ✅ Checklist

- [x] FileStorageService oluşturuldu
- [x] Static files middleware eklendi
- [x] KundeController upload/delete endpoint'leri
- [x] BenutzerController upload/delete endpoint'leri
- [x] Soft delete dosya silme entegrasyonu
- [x] Unit testler güncellendi
- [x] Null-safe işlemler
- [x] Path traversal koruması
- [x] Build başarılı ✅

---

## 🤝 Katkıda Bulunma

Sorular veya öneriler için issue açın!

---

**📝 Not:** Bu sistem local development ve test için hazır. Production'da external storage (Azure Blob, AWS S3) kullanmanız önerilir.
