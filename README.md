# PhotoDateFixer

![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)

**PhotoDateFixer** is a fast and lightweight multi-platform command-line tool built with .NET 8. It effectively restores the original "Creation" and "Modification" dates of your photos and videos.

Have you ever downloaded your entire media library from **Google Photos (Google Takeout)**, backed up files from **WhatsApp**, or moved your archives across old hard drives, only to find that all your photos are stamped with today's date? **PhotoDateFixer** solves this annoying problem by scanning the exact moment the photo or video was taken and restoring it directly to your file system.

## 🌟 Features

- **Intelligent Date Recovery**: Uses a reliable 3-tier fallback system to guarantee the most accurate capture date:
  1. **EXIF Metadata**: Deeply scans and reads the native embedded tags inside the image/video.
  2. **JSON Sidecars**: Fully supports extracting dates from Google Takeout's `*.json` metadata files.
  3. **Filename Parsing**: Extracts standard dates from conventional names like WhatsApp media (`VID-20250820-WA0001`) or Android Screenshots (`Screenshot_2026-05-06-23-32-20`).
- **Read-Only Bypass**: Encounters a locked or Read-Only file? No problem. It temporarily removes the attribute, updates the date, and restores the protection automatically.
- **Bilingual Interface**: Out-of-the-box support for English and Turkish.
- **Deep Recursive Scanning**: Processes all subfolders inside the selected root directory automatically.

## 📦 Supported Formats
- **Images:** `.jpg`, `.jpeg`, `.png`, `.heic`
- **Videos:** `.mp4`, `.mov`

## 🚀 How to Use / Installation

1. Go to the [Releases](https://github.com/metintnc/PhotoDateFixer/releases) page and download the latest executable.
2. Launch the application.
3. Select your language (Type `1` for English).
4. Enter or paste the full directory path containing your media files (e.g., `C:\Pictures\Backup`).
5. Sit back and watch! The tool will seamlessly process and update the original dates of your files.

---

# PhotoDateFixer (Türkçe)

**PhotoDateFixer**, fotoğraflarınızın ve videolarınızın kaybolan veya bozulan "Oluşturulma" ve "Değiştirilme" tarihlerini orijinal haline getiren, .NET 8 ile geliştirilmiş hızlı ve hafif bir araçtır.

Medya arşivinizi **Google Fotoğraflar'dan (Google Takeout)** indirdiğinizde, **WhatsApp** yedeklerinizi bilgisayara aktardığınızda veya harici diskler arasında taşıma yaptığınızda tüm fotoğrafların tarihinin kopyalandığı gün olarak ("bugün") değiştiğini fark ettiniz mi? **PhotoDateFixer**, medyanın tam olarak çekildiği tarihi bulur ve dosya sisteminize geri işleyerek kronolojinizi düzene sokar.

## 🌟 Özellikler

- **Akıllı Tarih Kurtarma**: En doğru tarihi bulmak için 3 aşamalı bir tarama mekanizması kullanır:
  1. **EXIF Verileri:** Resmin veya videonun içine gömülü olan orijinal çekim tarihini tarar ve okur.
  2. **JSON Dosyaları:** Google Takeout'un ürettiği harici `*.json` metadata dosyalarını analiz edip tarihi eşleştirir.
  3. **Dosya Adı Analizi:** İçinde veri bulunamasa bile WhatsApp (`VID-20250820-WA0001`) veya Ekran Görüntüsü (`Screenshot_2026-05-06...`) formatındaki dosya adlarından tarihi çözer.
- **Salt Okunur (Read-Only) Korumasını Aşma:** Sistemdeki eski ve kilitli (Salt okunur) dosyaları tanır. Özelliği geçici olarak kaldırır, işlemi tamamlar ve dosya güvenliği için tekrardan kilitler. Olası yetki hatalarını %100'e yakın engeller.
- **Çift Dil Desteği:** Başlangıçta İngilizce ve Türkçe olarak kullanım imkanı sunar.
- **Derin Klasör Tarama:** Seçtiğiniz ana klasörün içindeki tüm alt klasörleri hiyerarşik olarak otomatik tarar.

## 📦 Desteklenen Formatlar
- **Fotoğraflar:** `.jpg`, `.jpeg`, `.png`, `.heic`
- **Videolar:** `.mp4`, `.mov`

## 🚀 Nasıl Kullanılır?

1. GitHub [Releases](https://github.com/metintnc/PhotoDateFixer/releases) (Sürümler) sayfasından uygulamanın en son sürümünü indirin.
2. Uygulamayı çalıştırın.
3. Dil tercihinizi yapın (Türkçe için `2` yazıp Enter'a basın).
4. Fotoğraflarınızın bulunduğu klasörün tam yolunu yapıştırın (örneğin: `C:\Resimler\Yedekler`).
5. Arkanıza yaslanın! Sistem tüm dosyaları tarayacak ve gerçek tarihlerine geri döndürecektir.

## 🤝 Katkıda Bulunma
Geliştirmelere, hata raporlarına ve yeni özellik taleplerine her zaman açığız. GitHub üzerinden Pull Request (PR) gönderebilirsiniz.

## 📜 Lisans
Bu proje MIT Lisansı ile lisanslanmıştır. Detaylar için LICENSE dosyasına göz atabilirsiniz.
