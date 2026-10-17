<p align="center">
  <img src="docs/banner.png" alt="Faddi · فضّي" width="100%">
</p>

<p align="center">
  <a href="README.md">English</a> &middot;
  <a href="README.ar.md">العربية</a> &middot;
  <a href="README.es.md">Español</a> &middot;
  <a href="README.fr.md">Français</a> &middot;
  <a href="README.pt.md">Português</a> &middot;
  <a href="README.ru.md">Русский</a> &middot;
  <b>Türkçe</b> &middot;
  <a href="README.zh.md">简体中文</a>
</p>

<p align="center">
  <b>Diskini neyin yediğini bul, güvenle temizle ve RAM'ını kontrol altında tut.</b><br>
  Tüm diskini yaklaşık bir dakikada okuyan, bulduğu her şeyi silmenin ne kadar güvenli olduğuna göre sıralayan
  ve uygulamaların tutup kullanmadığı RAM'ı boşaltan ücretsiz, açık kaynaklı bir Windows uygulaması. Küçük tek bir exe, kurulacak bir şey yok.
</p>

<p align="center">
  <a href="https://github.com/sox29-ui/Faddi/releases/latest"><img alt="Son sürüm" src="https://img.shields.io/github/v/release/sox29-ui/Faddi?color=E8A33A&label=release"></a>
  <a href="https://github.com/sox29-ui/Faddi/releases"><img alt="İndirmeler" src="https://img.shields.io/github/downloads/sox29-ui/Faddi/total?color=3FBF8A"></a>
  <a href="LICENSE"><img alt="Lisans: MIT" src="https://img.shields.io/badge/license-MIT-2F6BDC"></a>
  <img alt="Windows 10 ve 11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078D4">
  <img alt=".NET 8" src="https://img.shields.io/badge/.NET-8-512BD4">
  <img alt="8 dil" src="https://img.shields.io/badge/languages-8-B67FD8">
  <img alt="Telemetri yok" src="https://img.shields.io/badge/telemetry-none-555">
</p>

<p align="center">
  <a href="#kurulum">Kurulum</a> &middot;
  <a href="#özellikler">Özellikler</a> &middot;
  <a href="#ekran-görüntüleri">Ekran görüntüleri</a> &middot;
  <a href="#faddi-neleri-temizler">Neleri temizler</a> &middot;
  <a href="#ram-optimizasyonu">RAM optimizasyonu</a> &middot;
  <a href="#tasarım-gereği-güvenli">Tasarım gereği güvenli</a> &middot;
  <a href="#geliştiriciler-için">Geliştiriciler için</a> &middot;
  <a href="CONTRIBUTING.md">Katkıda bulunma</a>
</p>

<p align="center">
  <img src="docs/home-en.png" alt="Faddi'nin ana sayfası: tek bir şerit diski oyunlara, geliştirici kalıntılarına, önbelleklere, eski şeylere, Windows'a ve boş alana böler" width="900">
</p>

---

## Faddi nedir?

Faddi (فضّي, Arapçada "boşalt" demektir) disk alanının tam olarak nereye gittiğini gösterir ve hiçbir şeyi bozmadan onu geri kazanmana yardım eder. Tüm diski okur; oyunlara ve uygulamalara, geliştirici kalıntılarına, önbelleklere, eski dosyalara ve Windows'un kendisine ayırır, her şeyin ne olduğunu ve silersen ne olacağını söyler.

- **Alanın nereye gittiğini gör**: tüm disk için tek bir şerit, her klasörün haritası, en büyük dosyalar ve yinelenenler.
- **Güvenle temizle**: önbellekler önceden işaretli gelir, geri kalan her şey senin kararını bekler, kişisel dosyalar da Geri Dönüşüm Kutusu'na gider.
- **RAM'ı kontrol altında tut**: RAM'ı tek tıkla ya da otomatik olarak boşalt, ani bir artışa hangi uygulamanın yol açtığını gör.
- **Bilgisayarını tanı**: işlemci, RAM, ekran kartı, sağlık durumuyla diskler, ağ ve başlangıç uygulamaları; hepsi sade bir dille açıklanır.

Her şey bilgisayarında kalır. Faddi ağ üzerinden hiçbir şey göndermez.

## Kurulum

### PowerShell'de tek satır (en kolayı, uyarı ekranı yok)

Başlat'a bas, `PowerShell` yaz ve Enter'a bas. Sonra şu satırı yapıştır ve yine Enter'a bas:

```powershell
irm https://raw.githubusercontent.com/sox29-ui/Faddi/main/install.ps1 | iex
```

En son `Faddi.exe` dosyasını `%LOCALAPPDATA%\Programs\Faddi` klasörüne indirir, Faddi'yi Başlat menüsüne ve masaüstüne ekler, sonra açar. Güncellemek için aynı satırı yeniden çalıştır.

### Ya da exe'yi indir

<p>
  <a href="https://github.com/sox29-ui/Faddi/releases/latest/download/Faddi.exe"><img alt="Faddi.exe'yi indir" src="https://img.shields.io/badge/%C4%B0ND%C4%B0R-Faddi.exe-E8A33A?style=for-the-badge"></a>
</p>

1. [Son sürümden](https://github.com/sox29-ui/Faddi/releases/latest) **Faddi.exe** dosyasını indir. Her şeyi içinde barındıran, yaklaşık 66 MB boyutunda tek bir dosyadır; yani kurulacak bir şey yok.
2. Çift tıkla. Tarama hemen başlar: NVMe SSD'deki beş milyon dosya 45 ile 90 saniye arasında sürer ve disk şeridi, dosyalar okunurken dolar.

   Exe yeni olduğu ve kod imzalı olmadığı için Windows **"Windows kişisel bilgisayarınızı korudu"** uyarısını gösterebilir. **Ek bilgi** bağlantısına, sonra **Yine de çalıştır** düğmesine tıkla. Yukarıdaki PowerShell satırıyla bu ekranı hiç görmezsin.
3. İsteğe bağlı: kenar çubuğunun altındaki **Yönetici olarak çalıştır** düğmesine bas. Yönetici olarak Faddi korumalı klasörleri okuyabilir, disk sıcaklığını ve aşınmasını gösterebilir, Windows'un kendi kalıntılarını temizleyebilir ve bekleme önbelleğini boşaltabilir.

> Faddi, Windows 11 ile birlikte gelen ve hemen her Windows 10 bilgisayarında zaten bulunan Microsoft Edge WebView2 Runtime bileşenini kullanır. Eksikse Faddi sana söyler.<br>
> Faddi'yi kaldırmak için RAM ve performans sayfasında **Windows ile birlikte saatin yanında başlat** seçeneğini kapat, sonra `Faddi.exe` dosyasını (ya da `%LOCALAPPDATA%\Programs\Faddi` klasörünü), kısayollarını ve `%LOCALAPPDATA%\Faddi` klasörünü sil.

## Özellikler

| Özellik | Neler yapabilirsin |
|---|---|
| <img src="docs/icons/strip.svg" width="22"> **Tek bakışta disk** | Tek bir şerit diski şu parçalara böler: oyunlar ve uygulamalar, geliştirici kalıntıları, önbellek ve geçici dosyalar, eski ve büyük şeyler, Windows ve diğer her şey. |
| <img src="docs/icons/map.svg" width="22"> **Alan haritası** | Her klasör, kapladığı alanla orantılı büyüklükte bir kutucuk olarak görünür; rengi, içindeki herhangi bir şeyin en son ne zaman değiştiğini gösterir. İçine girmek için bir kutucuğa tıkla. |
| <img src="docs/icons/clean.svg" width="22"> **Güvenliğe göre temizlik** | Önbellekler ve geçici dosyalar önceden işaretli gelir; geliştirici kalıntıları, büyük oyunlar ve 6 aydır dokunulmamış klasörler senin kararını bekler. Her öğe ne olduğunu ve silersen ne olacağını söyler. |
| <img src="docs/icons/code.svg" width="22"> **Geliştirici kalıntıları** | lockfile ile aynı klasördeki `node_modules`, Python ortamları (venv), Unity `Library`, Unreal `DerivedDataCache`, .NET `bin`/`obj`, Gradle, Flutter ve Rust derleme çıktıları ile npm, pip, uv, NuGet, Gradle ve Hugging Face önbellekleri. |
| <img src="docs/icons/files.svg" width="22"> **En büyük dosyalar ve türler** | Diskteki en büyük dosyalar ve en çok yeri hangi tür dosyaların kapladığı. |
| <img src="docs/icons/dupes.svg" width="22"> **Yinelenen dosyalar** | Kopyalar adlara değil, içeriğe bakılarak bulunur. Yalnızca kopya gibi görünen sabit bağlantılar (hard link) listeye alınmaz. |
| <img src="docs/icons/apps.svg" width="22"> **Yüklü uygulamalar** | Her uygulama diskteki gerçek boyutuyla listelenir; yanında uygulamanın kendi kaldırma aracını açan bir düğme bulunur. |
| <img src="docs/icons/bolt.svg" width="22"> **RAM optimizasyonu** | RAM'ı tek tıkla boşaltır; kullanım bir sınırı aşınca ya da birkaç dakikada bir kendiliğinden de boşaltır, ama otomatik çalışma yalnızca işlemci boştayken olur. [Daha fazlası aşağıda](#ram-optimizasyonu). |
| <img src="docs/icons/ram.svg" width="22"> **RAM izleme** | 2 saniyede bir ölçüm alır, ani artışları ve arkasındaki uygulamaları kaydeder; ayrıca WSL sınırı, sızıntı yapan bir aygıt sürücüsü ya da açık unutulmuş bir Gradle daemon işlemi gibi konularda ipuçları verir. |
| <img src="docs/icons/sys.svg" width="22"> **Sistem bilgileri** | İşlemci, RAM modülleri, NVIDIA'da canlı sıcaklığı görünen ekran kartı, sağlık durumu, sıcaklık ve aşınma bilgisiyle diskler, ağ, ekranlar, pil ve başlangıç uygulamaları; her birinin yanında kısa bir açıklama var. |
| <img src="docs/icons/tray.svg" width="22"> **Saatin yanında** | Pencereyi kapatınca Faddi sistem tepsisinde kalır ve RAM'ı izler. Windows ile birlikte başlayabilir. |
| <img src="docs/icons/globe.svg" width="22"> **Sekiz dil** | English, العربية, Español, Français, Português, Русский, Türkçe ve 简体中文; Arapça için sağdan sola. Windows'un diliyle açılır. |

## Ekran görüntüleri

<table>
  <tr>
    <td width="50%"><img src="docs/home-en.png" alt="Ana sayfa"><p align="center">Ana sayfa: tüm disk tek bir şeritte</p></td>
    <td width="50%"><img src="docs/map-en.png" alt="Alan haritası"><p align="center">Alan haritası, yaşa göre renkli</p></td>
  </tr>
  <tr>
    <td><img src="docs/clean-en.png" alt="Temizlik"><p align="center">Temizlik, güvenlik düzeyine göre sıralı</p></td>
    <td><img src="docs/ram-en.png" alt="RAM optimizasyonu"><p align="center">RAM optimizasyonu</p></td>
  </tr>
  <tr>
    <td><img src="docs/sys-en.png" alt="Sistem bilgileri"><p align="center">Sistem bilgileri, canlı</p></td>
    <td><img src="docs/home-ar.png" alt="Arapça ana sayfa"><p align="center">Arapça, sağdan sola</p></td>
  </tr>
  <tr>
    <td><img src="docs/home-zh.png" alt="Çince ana sayfa"><p align="center">简体中文</p></td>
    <td><img src="docs/home-ru.png" alt="Rusça ana sayfa"><p align="center">Русский</p></td>
  </tr>
</table>

## Faddi neleri temizler?

Faddi'nin bulduğu her şey beş gruptan birine girer; renkleri disk şeridindekilerle aynıdır.

| Grup | İçinde ne var | Temizleyince ne olur |
|---|---|---|
| <img src="docs/icons/dot-safe.svg" width="12"> **Güvenli temizlik** | Bir haftadan eski geçici dosyalar, tarayıcı ve ekran kartı gölgelendirici önbellekleri ve uygulamaların yeniden oluşturduğu diğer önbellekler | Silinir. Uygulamalar gerektiğinde yeniden oluşturur. Önceden işaretli gelir. |
| <img src="docs/icons/dot-dev.svg" width="12"> **Geliştirici kalıntıları** | `node_modules`, Python ortamları (venv), derleme klasörleri ve paket önbellekleri | Silinir. `npm install` gibi tek bir komutla geri gelirler. |
| <img src="docs/icons/dot-games.svg" width="12"> **Oyunlar ve uygulamalar** | Yüklü büyük oyunlar ve uygulamalar | Kendi kaldırma aracıyla ya da Windows Ayarları'ndan kaldırılır. |
| <img src="docs/icons/dot-review.svg" width="12"> **Eski ve büyük şeyler** | 6 aydır dokunulmamış klasörler, eski indirilenler ve videolar | Geri Dönüşüm Kutusu'na taşınır, böylece geri yükleyebilirsin. |
| <img src="docs/icons/dot-system.svg" width="12"> **Windows ve sistem** | pagefile.sys, hazırda bekletme dosyası, WinSxS, geri yükleme noktaları | Yalnızca Windows'un kendi araçlarıyla değiştirilir; her değişikliğin ne anlama geldiği anlatılır. |

## RAM optimizasyonu

**RAM ve performans** sayfasından, kenar çubuğundaki **Optimize et** düğmesinden ya da sistem tepsisi simgesinden aç. Her uygulamadan tutup kullanmadığı RAM'ı geri vermesini ister; uygulamalar yalnızca gerçekten ihtiyaç duyduklarını geri alır, böylece bir oyun ya da ağır bir uygulama için daha fazla yer kalır.

| Ayar | Ne yapar |
|---|---|
| <img src="docs/icons/bolt.svg" width="16"> **RAM'ı şimdi optimize et** | RAM'ı hemen boşaltır ve ne kadar yer açıldığını gösterir. |
| **RAM kullanımı … olunca** | Kullanım seçtiğin sınırı aşınca kendiliğinden çalışır (%70 ile %95 arası, varsayılan %85), en fazla 2 dakikada bir. |
| **Her …** | Zamanlayıcıyla çalışır: her 3, 5, 10, 30 ya da 60 dakikada bir. |
| **Yalnızca işlemci boştayken** | Otomatik çalıştırmalar işlemci %30'un altına inene kadar bekler, bu yüzden bir oyunun performansını hiç düşürmez. Varsayılan olarak açık. |
| **Uygulamaların tuttuğu RAM'ı boşalt** | Her uygulamanın çalışma kümesini (working set) boşaltır: tuttuğu ama kullanmadığı sayfalar Windows'a geri döner. Varsayılan olarak açık. |
| **Windows bekleme önbelleğini temizle** | Yönetici olarak, Windows'un RAM'da tuttuğu, yakın zamanda kullanılmış dosyaların önbelleğini de boşaltır. |
| **Panoyu temizle** | En son kopyaladığın şeyi temizler; bu, büyük bir resim olabilir. |
| **Bana bildir** | Her otomatik çalıştırmadan sonra küçük bir bildirim gösterir. |
| <img src="docs/icons/tray.svg" width="16"> **Pencereyi kapatınca saatin yanında çalışmaya devam et** | Pencereyi kapatmak Faddi'yi sistem tepsisinde bırakır; RAM'ı izlemeyi ve optimizasyonu çalıştırmayı sürdürür. Varsayılan olarak açık. |
| **Windows ile birlikte saatin yanında başlat** | Oturum açtığında sistem tepsisinde başlar. |

> Boşaltılan RAM, uygulamalar ona yeniden eriştikçe kısmen tekrar dolar; bu beklenen bir durum. Windows RAM'ı kendi başına iyi yönetir, bu yüzden optimizasyon en çok bir oyundan ya da ağır bir uygulamadan hemen önce veya bir şey RAM'ı uzun süre doldurup durduktan sonra işe yarar.

## Tasarım gereği güvenli

- **Seçilenleri temizle** düğmesine basıp onaylamadan hiçbir şey silinmez. Kişisel dosyalar (eski klasörler, indirilenler, videolar) Geri Dönüşüm Kutusu'na gider; kutuya sığmayacak kadar büyük bir şey olursa Windows önceden uyarır.
- Junction'ları ve sembolik bağlantıları (symlink) asla izlemez, bu yüzden `node_modules` temizlenirken başka bir yerdeki bağlantılı bir klasöre ulaşılamaz.
- Windows, Program Files, ProgramData\Microsoft, her kullanıcı profili ve onun Masaüstü, Belgeler ve AppData klasörleri silmeye kapalıdır; `C:\PROGRA~1` gibi kısa adlar kontrol edilmeden önce gerçek yollarına çözülür.
- Temp klasöründe son 7 gün içinde değişen her şey korunur; çalışan bir uygulamanın kullandığı dosyalar atlanır. Outlook'un ek önbelleğine dokunulmaz.
- Sistem dosyaları (pagefile.sys, hazırda bekletme dosyası, WinSxS, geri yükleme noktaları) yalnızca Windows'un kendi araçlarıyla değiştirilir.
- Windows'un temel işlemleri RAM ve performans sayfasından kapatılamaz.

## Gereksinimler

- **Windows 10 veya 11, 64 bit.**
- **Microsoft Edge WebView2 Runtime**: Windows 11'in içindedir, hemen her Windows 10 bilgisayarında da zaten vardır.
- **Yönetici yetkisi**: isteğe bağlı. Her şey onsuz da çalışır; birkaç ek özellik için gerekir.
- **NVIDIA ekran kartı**: isteğe bağlı; ekran kartının canlı sıcaklığını, fanını ve gücünü görmek için.

## Geliştiriciler için

[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ve Windows gerekir.

```bash
git clone https://github.com/sox29-ui/Faddi.git
cd Faddi
dotnet run --project Faddi           # uygulamayı çalıştır
dotnet run --project Faddi.Check     # öz denetimler: tarama, temizlik kuralları ve silme koruması, geçici deneme klasörlerinde
node tools/i18n-check.mjs            # her dilde her metin var mı, {0} yer tutucularıyla birlikte
```

Sürümlerle birlikte gelen tek `Faddi.exe` dosyasını derlemek için:

```bash
dotnet publish Faddi -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none -o publish
```

### Proje yapısı

```text
Faddi/
  Faddi/                  uygulama
    Program.cs            pencere, sistem tepsisi simgesi, tek örnek denetimi ve sayfanın konuştuğu köprü
    Scanner.cs            gerçek disk boyutlarıyla paralel, tüm diski kapsayan tarama
    Cleanup.cs            temizlik kuralları ve neyin silinebileceğine karar veren koruma
    Dupes.cs              yinelenen dosya bulucu: önce örneklenmiş hash, sonra tam SHA-256
    Optimizer.cs          RAM optimizasyonu
    Watcher.cs            işlemci, RAM, ekran kartı, disk ve ağ ölçümü ile RAM ani artış günlüğü
    SysInfo.cs            WMI, kayıt defteri ve nvidia-smi ile toplanan sistem bilgileri
    Settings.cs, I18n.cs  ayarlar ve çeviriler
    index.html            arayüzün tamamı, tek sayfa
    lang/                 dil başına bir JSON dosyası
  Faddi.Check/            öz denetimler
  tools/i18n-check.mjs    çeviri denetleyicisi
  docs/                   bu README için logo, simgeler ve ekran görüntüleri
```

### Kullanılan teknolojiler

.NET 8 WinForms bir WebView2 sayfasını barındırır; web sunucusu da ağ da yoktur. Tarama, optimizasyon ve izleme doğrudan Windows ile konuşur: tarama için `FindFirstFileExW`, bellek için `EmptyWorkingSet` ve `NtSetSystemInformation`, canlı rakamlar için PDH performans sayaçları, donanım için WMI. Arayüz, hiçbir framework kullanmayan tek bir HTML dosyasıdır.

### Dil ekle

Arapça doğrudan kodun içinde yazılıdır; diğer her dil `Faddi/lang` içinde tek bir dosyadır. `en.json` dosyasını kendi dilinin koduyla kopyala (örneğin `de.json`), değerleri çevir, `"@name"` alanını dilin kendi adına ayarla ve `node tools/i18n-check.mjs` komutunu eksik ya da bozuk bir şey bildirmeyene kadar çalıştır. Yeni dil, kenar çubuğunun altındaki dil seçicide görünür.

## Verilerin

- Ayarlar `%LOCALAPPDATA%\Faddi\settings.json` dosyasında, RAM ani artış günlüğü ise yanındaki `spikes.json` dosyasında durur.
- Faddi hiçbir şeyi internete yüklemez; hesap, reklam ya da telemetri de yoktur.

## Katkıda bulunma

Issue'lar ve pull request'ler memnuniyetle karşılanır; özellikle yeni diller ve Faddi'nin henüz tanımadığı araçlar için temizlik kuralları. [Katkı rehberiyle](CONTRIBUTING.md) başla ve lütfen [davranış kurallarına](CODE_OF_CONDUCT.md) uy.

Bir güvenlik sorunu mu buldun, örneğin Faddi'nin silmemesi gereken bir şeyi sildirmenin bir yolu? Lütfen [SECURITY.md](SECURITY.md) dosyasında anlatıldığı gibi özel olarak bildir.

## Lisans

Faddi, [MIT Lisansı](LICENSE) ile yayımlanmıştır. Birlikte gelen kütüphaneler kendi lisanslarını korur; bunlar [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) dosyasında listelenir.
