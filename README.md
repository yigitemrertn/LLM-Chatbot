# LLM Chatbot - Modern Chat Interface Uygulamasý

Bir Windows Forms tabanlý, modern LLM (Large Language Model) sohbet arayüzüdür. OpenAI API'sine entegre edilebildiði gibi mock servis ile test edilebilir.

## ?? Özellikler

### 1. **Modern Sohbet Arayüzü**
- Clean ve modern tasarým
- Chat bubble mantýðý ile mesaj gösterimi
- Renk kodlu kullanýcý ve asistan mesajlarý
- Zaman damgasý ile her mesaj
- RichTextBox tabanlý sohbet ekraný

### 2. **ChatService - LLM Entegrasyonu**
- **OpenAI API Entegrasyonu**: Gerçek API çaðrýlarý yapabilir
- **Mock Servis**: API olmadan test edebilirsiniz
- Ýstisnai durum yönetimi (exception handling)
- Timeout ve hata yönetimi
- Model ve parametreleri ayarlayabilme (temperature, max tokens)

### 3. **Konversasyon Baðlamý (Context Awareness)**
- Konversasyon geçmiþi hafýzada tutma
- Son 10 mesaj konteksti ile API'ye gönderme
- Konuþma istatistikleri
- Konuþma ID'si ve oluþturulma zamaný

### 4. **Güvenli API Anahtar Yönetimi**
- DPAPI (Data Protection API) ile þifrelenmiþ depolama
- AppData klasöründe güvenli kaydedilme
- Sadece geçerli kullanýcýda deþifre edilebilir
- Kilit açma (remove/update) desteði

### 5. **Ayarlar Paneli**
- API anahtarý ayarlama
- Model seçimi (gpt-4, gpt-3.5-turbo, gpt-3.5-turbo-16k)
- Temperature (sýcaklýk) kontrolü (0.0 - 2.0)
- Baðlantý testi

## ?? Dosya Yapýsý

```
LLM Chatbot/
??? Models/
?   ??? ChatMessage.cs          # Mesaj modeli (content, role, timestamp)
?   ??? ConversationContext.cs  # Konversasyon geçmiþi yönetimi
??? Services/
?   ??? ConfigurationManager.cs # Güvenli ayar yönetimi (DPAPI)
?   ??? ChatService.cs          # OpenAI API ve Mock servis
??? ChatbotForm.cs              # Ana sohbet arayüzü
??? ChatbotForm.Designer.cs     # Form tasarýmý
??? SettingsForm.cs             # Ayarlar formu
??? SettingsForm.Designer.cs    # Ayarlar tasarýmý
??? Program.cs                  # Uygulamasý giriþ noktasý
```

## ?? Kullaným

### Temel Kullaným
1. Uygulamayý baþlatýn
2. "Ayarlar" butonuna týklayýn
3. OpenAI API anahtarýnýzý girin veya mock mode'da kalýn
4. Mesaj yazýn ve "Gönder" butonuna týklayýn veya Ctrl+Enter tuþuna basýn
5. Asistandan cevap alýn

### Mock Mode (Test) - Varsayýlan
```csharp
// ChatbotForm.cs içinde (þu anda aktif):
_chatService = new Services.ChatService(useMockService: true);
```
Mock mode'da gerçek API'ye ihtiyaç yoktur. Test amaçlý cevaplarý simüle eder.

### Gerçek OpenAI API Kullanma
```csharp
// ChatbotForm.cs içinde useMockService: true deðerini false olarak deðiþtirin:
_chatService = new Services.ChatService(useMockService: false);
```
OpenAI API anahtarýný ayarlar panelinden ayarlayýn.

## ?? Güvenlik Özellikleri

### DPAPI Þifreleme
API anahtarlarý DPAPI kullanýlarak þifrelenir:
```csharp
// Þifreleme
byte[] dataToEncrypt = Encoding.UTF8.GetBytes(data);
byte[] encryptedData = ProtectedData.Protect(dataToEncrypt, null, DataProtectionScope.CurrentUser);

// Deþifreleme
byte[] decryptedData = ProtectedData.Unprotect(dataToEncrypt, null, DataProtectionScope.CurrentUser);
```

### Ýstisnai Durum Yönetimi
Tüm kritik iþlemler try-catch bloklarýyla korunmaktadýr:
- HTTP istek hatalarý
- Timeout yönetimi
- JSON parsing hatalarý
- Dosya I/O hatalarý
- TextBox kontrolünün doðru referanslanmasý

## ?? Sistem Gereksinimleri

- .NET 10.0 Windows
- Windows Forms desteði
- Ýnternet baðlantýsý (OpenAI API'si için)

## ?? Ayar Dosyasý

Ayarlar aþaðýda kaydedilir:
```
C:\Users\[YourUsername]\AppData\Roaming\LLM_Chatbot\config.json
```

## ?? API Yanýt Formatý

OpenAI API'sinden beklenen format:
```json
{
  "choices": [
    {
      "message": {
        "content": "API yanýtý"
      }
    }
  ]
}
```

## ?? UI Özellikleri

- **Tema Renkleri**:
  - Ana arka plan: Hafif gri (RGB: 240, 242, 245)
  - Kullanýcý mesajlarý: Mavi (RGB: 59, 130, 246)
  - Asistan mesajlarý: Yeþil (RGB: 34, 197, 94)
  - Status bar: Koyu gri (RGB: 31, 41, 55)

- **Butonlar**:
  - Gönder: Mavi
  - Temizle: Gri
  - Ayarlar: Mor
  - Kaydet/Baðlantýyý Test Et: Özelleþtirilmiþ renkler

## ?? Hata Yönetimi

Uygulama aþaðýdaki hatalarý iþler:
1. **NetworkError**: Ýnternet baðlantýsý sorunu
2. **AuthenticationError**: Geçersiz API anahtarý
3. **TimeoutError**: API yanýt verme süresi aþýldý
4. **ValidationError**: Boþ mesaj gönderme giriþimi
5. **ConfigurationError**: Ayar dosyasý okuma/yazma hatasý

## ?? Konversasyon Ýstatistikleri

Konuþma baðlamýnda mevcut:
- Toplam mesaj sayýsý
- Kullanýcý mesajlarý
- Asistan mesajlarý
- Toplam kelime sayýsý
- Konuþma süresi

```csharp
var stats = _conversationContext.GetStatistics();
```

## ?? Ýþ Akýþý

1. **Kullanýcý mesaj gönderir** ? Sohbet penceresine eklenir
2. **Konversasyon kontekstine kaydedilir** ? Status "Ýþleniyor..."
3. **ChatService mesajý iþler** ? OpenAI/Mock servisine gönderilir
4. **Yanýt alýnýr** ? Sohbet penceresine eklenir, kontekste kaydedilir
5. **Status "Hazýr"** ? Yeni mesaj için hazýr

## ?? Dependencies

- System.Net.Http (yerleþik)
- System.Text.Json (yerleþik)
- System.Security.Cryptography (DPAPI için)

## ?? Eðitim Amaçlý

Bu proje þunlarý öðretir:
- Windows Forms tabanlý UI tasarýmý
- Async/await asenkron programlama
- HTTP istek yapma (HttpClient)
- JSON iþleme
- Veri þifreleme (DPAPI)
- Ýstisnai durum yönetimi
- Mimari pattern (Service, Context, Configuration)
- UI kontrol referanslarý ve lifecycle

## ? Son Güncellemeler

### v1.1.0 - Bug Fix
- **Düzeltme**: Mesaj gönderme butonundaki kontrol referansý sorunu çözüldü
- TextBox kontrolüne doðru þekilde eriþim saðlanýyor
- Türkçe interface desteði eklendi
- Daha basit ve güvenilir UI implementasyonu

---

**Geliþtirici**: [Your Name]
**Sürüm**: 1.1.0
**Son Güncelleme**: 2024
