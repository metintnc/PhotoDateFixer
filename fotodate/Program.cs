using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using ImageMagick;

namespace fotodate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Select Language / Dil Seçin:");
            Console.WriteLine("1 - English");
            Console.WriteLine("2 - Türkçe");
            string? langChoice = Console.ReadLine();
            bool isEng = langChoice?.Trim() == "1";

            Console.WriteLine(isEng ? "Please enter the path to the folder containing the photos (e.g., C:\\Pictures):" : "Lütfen fotoğrafların bulunduğu klasörün yolunu girin (örneğin: C:\\Resimler):");
            string? inputPath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(inputPath) || !System.IO.Directory.Exists(inputPath))
            {
                Console.WriteLine(isEng ? "You entered an invalid folder path." : "Geçersiz klasör yolu girdiniz.");
                return;
            }

            string[] extensions = { ".jpg", ".jpeg", ".png", ".heic", ".mp4", ".mov" };
            var files = System.IO.Directory.GetFiles(inputPath, "*.*", SearchOption.AllDirectories)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                .ToList();

            Console.WriteLine(isEng ? $"{files.Count} supported files found. Process is starting..." : $"{files.Count} adet desteklenen dosya bulundu. İşlem başlatılıyor...");

            int successCount = 0;
            int failCount = 0;
            int processedCount = 0;
            int totalFiles = files.Count;

            foreach (var file in files)
            {
                processedCount++;
                double percentage = ((double)processedCount / totalFiles) * 100;

                // Konsol başlığında yüzdeyi göster
                Console.Title = isEng ? $"fotodate - Progress: {percentage:F1}%" : $"fotodate - İlerleme: %{percentage:F1}";

                // Öncelik Sırası: Önce JSON -> Sonra Dosya İsmi -> En son EXIF
                DateTime? dateTaken = GetDateFromJson(file) ?? GetDateFromFileName(file) ?? GetDateFromExif(file);

                string logMessage = "";

                if (dateTaken.HasValue)
                {
                    try
                    {
                        // Salt okunur (Read-Only) kontrolü ve geçici olarak kaldırılması
                        var attributes = File.GetAttributes(file);
                        bool isReadOnly = (attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                        if (isReadOnly)
                        {
                            File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
                        }

                        File.SetCreationTime(file, dateTaken.Value);
                        File.SetLastWriteTime(file, dateTaken.Value);

                        // Fotoğrafın içine (EXIF) tarihi yaz
                        WriteExifDate(file, dateTaken.Value);

                        // Dosya aslında salt okunursa eski haline geri getir
                        if (isReadOnly)
                        {
                            File.SetAttributes(file, attributes);
                        }

                        logMessage = isEng ? $"[SUCCESS] {Path.GetFileName(file)} -> {dateTaken.Value}" : $"[BAŞARILI] {Path.GetFileName(file)} -> {dateTaken.Value}";
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        logMessage = isEng ? $"[ERROR] {Path.GetFileName(file)} could not be updated: {ex.Message}" : $"[HATA] {Path.GetFileName(file)} güncellenemedi: {ex.Message}";
                        failCount++;
                    }
                }
                else
                {
                    logMessage = isEng ? $"[SKIPPED] No date found for {Path.GetFileName(file)}." : $"[ATLANDI] {Path.GetFileName(file)} için tarih bulunamadı.";
                    failCount++;
                }

                // Bulunduğumuz satırı temizle (eski yüzde yazısını silmek için)
                int windowWidth = Console.WindowWidth > 0 ? Console.WindowWidth - 1 : 50;
                Console.Write("\r" + new string(' ', windowWidth) + "\r");

                // İşlem logunu bir alt satıra yazdır
                Console.WriteLine(logMessage);

                // En alta sürekli yenilenecek olan yüzdeyi yaz (imleç aynı satırda kalsın diye WriteLine kullanmıyoruz)
                Console.Write(isEng ? $"Progress: {percentage:F1}%" : $"İlerleme: %{percentage:F1}");
            }

            Console.WriteLine(); // İşlem bitince son yüzde satırının altına geç

            Console.Title = isEng ? "fotodate - Completed" : "fotodate - Tamamlandı";
            Console.WriteLine(isEng ? $"Process completed. {successCount} successful, {failCount} failed/skipped." : $"İşlem tamamlandı. {successCount} başarılı, {failCount} başarısız/atlanan.");
            Console.WriteLine(isEng ? "Press any key to exit..." : "Çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }

        static DateTime? GetDateFromExif(string filePath)
        {
            try
            {
                var directories = ImageMetadataReader.ReadMetadata(filePath);
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                if (subIfdDirectory != null && subIfdDirectory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out DateTime dateTime))
                {
                    return dateTime;
                }
            }
            catch
            {
                // Metadata okuma hatası durumunda sessizce geç
            }
            return null;
        }

        static void WriteExifDate(string filePath, DateTime date)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
            {
                try
                {
                    using (var image = new MagickImage(filePath))
                    {
                        var profile = image.GetExifProfile();
                        if (profile == null)
                        {
                            profile = new ExifProfile();
                        }
                        
                        string exifStr = date.ToString("yyyy:MM:dd HH:mm:ss");
                        profile.SetValue(ExifTag.DateTimeOriginal, exifStr);
                        profile.SetValue(ExifTag.DateTimeDigitized, exifStr);
                        profile.SetValue(ExifTag.DateTime, exifStr);
                        
                        image.SetProfile(profile);
                        image.Write(filePath);
                    }
                }
                catch
                {
                    // Bazı dosyalar desteklenmeyebilir veya bozuk olabilir, hata verirsek devam etsin
                }
            }
        }

        static DateTime? GetDateFromJson(string filePath)
        {
            try
            {
                // Google Photos genellikle "resim.jpg.json" şeklinde metadata çıkartır.
                string jsonPath = filePath + ".json";
                DateTime? date = ParseDateFromJsonFile(jsonPath);
                if (date.HasValue) return date;

                // Bazen uzantı olmadan "resim.json" şeklinde de olabilir.
                string? dir = Path.GetDirectoryName(filePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                if (dir != null)
                {
                    string altJsonPath = Path.Combine(dir, fileNameWithoutExt + ".json");
                    if (altJsonPath != jsonPath)
                    {
                        return ParseDateFromJsonFile(altJsonPath);
                    }
                }
            }
            catch
            {
                // JSON işleme hatası durumunda sessizce geç
            }
            return null;
        }

        static DateTime? ParseDateFromJsonFile(string jsonPath)
        {
            if (File.Exists(jsonPath))
            {
                string jsonContent = File.ReadAllText(jsonPath);
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    if (doc.RootElement.TryGetProperty("photoTakenTime", out JsonElement photoTakenTimeElement))
                    {
                        if (photoTakenTimeElement.TryGetProperty("timestamp", out JsonElement timestampElement))
                        {
                            string? tsString = timestampElement.GetString();
                            if (tsString != null && long.TryParse(tsString, out long timestamp))
                            {
                                return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToLocalTime();
                            }
                        }
                    }
                }
            }
            return null;
        }

        static DateTime? GetDateFromFileName(string filePath)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                // Pattern 1: YYYYMMDD_HHMMSS (örn: VID_20260103_223956)
                var match1 = Regex.Match(fileName, @"(20\d{2})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})");
                if (match1.Success)
                {
                    if (DateTime.TryParseExact(match1.Value, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                        return dt;
                }

                // Pattern 2: Whatsapp (örn: VID-20250820-WA0001)
                var match2 = Regex.Match(fileName, @"(20\d{2})(\d{2})(\d{2})-WA");
                if (match2.Success)
                {
                    string dateStr = match2.Groups[1].Value + match2.Groups[2].Value + match2.Groups[3].Value;
                    if (DateTime.TryParseExact(dateStr, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                        return dt;
                }

                // Pattern 3: Screenshot (örn: Screenshot_2026-05-06-23-32-20)
                var match3 = Regex.Match(fileName, @"(20\d{2})-(\d{2})-(\d{2})-(\d{2})-(\d{2})-(\d{2})");
                if (match3.Success)
                {
                    if (DateTime.TryParseExact(match3.Value, "yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                        return dt;
                }

                // Pattern 4: Screenshot (örn: Screenshot_20190903-172454)
                var match4 = Regex.Match(fileName, @"(?<!\d)(20\d{2})(\d{2})(\d{2})-(\d{2})(\d{2})(\d{2})(?!\d)");
                if (match4.Success)
                {
                    if (DateTime.TryParseExact(match4.Value, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                        return dt;
                }

                // Pattern 5: Düz yyyyMMdd (örn: 20211117-1, 20211117-2)
                var match5 = Regex.Match(fileName, @"(?<!\d)(20\d{2})(\d{2})(\d{2})(?!\d)");
                if (match5.Success)
                {
                    if (DateTime.TryParseExact(match5.Value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                        return dt;
                }
            }
            catch
            {
                // Sessizce geç
            }
            return null;
        }
    }
}
