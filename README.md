<div align="center">

# 🧾 KasirKu

**Aplikasi Kasir Desktop yang Ringan, Cepat, dan Ramah untuk PC Spesifikasi Rendah**

Dibangun dengan C# & WPF — dirancang untuk UMKM, toko kelontong, dan retail kecil.

![Status](https://img.shields.io/badge/status-in%20development-yellow)
![Platform](https://img.shields.io/badge/platform-Windows-0078D6)
![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![License](https://img.shields.io/badge/license-MIT-green)

</div>

---

## 📌 Tentang KasirKu

Banyak aplikasi POS di pasaran terlalu berat untuk PC-PC lama yang masih dipakai toko kecil sehari-hari. **KasirKu** hadir sebagai jawaban: aplikasi kasir desktop native yang tetap gesit di PC dengan RAM 2GB sekalipun, tanpa mengorbankan fitur yang dibutuhkan kasir di lapangan.

Proyek ini juga jadi wadah belajar pengembangan aplikasi desktop dari nol — mulai dari perancangan database, arsitektur MVVM, sampai integrasi hardware seperti printer thermal dan barcode scanner.

## ✨ Fitur Utama

| Fitur | Deskripsi |
|---|---|
| **Transaksi Cepat** | Scan barcode, cari produk, hitung total — semua bisa full keyboard, tanpa mouse |
| **Manajemen Stok** | Tambah/kurang stok otomatis, alert saat stok menipis |
| **Harga Historis** | Laporan laba-rugi tetap akurat meski harga produk berubah dari waktu ke waktu |
| **Cetak Struk Thermal** | Dukungan langsung ke printer 58mm/80mm via ESC/POS, bukan cuma driver Windows biasa |
| **Hold Transaksi** | Tahan transaksi sementara saat pembeli lupa ambil barang, lanjut ke antrean berikutnya |
| **Laporan** | Ringkasan penjualan harian & bulanan, produk terlaris, stok kritis |
| **Lacak & Audit Shift Kasir** | Memantau aktivitas shift kasir dan melakukan audit modal awal dan akhir shift |
| **Backup Sekali Klik** | Export database & laporan ke file lokal (CSV/Excel), tanpa perlu server |
| **Aman Saat Mati Listrik** | SQLite dengan mode WAL — data transaksi tahan dari crash mendadak |

## ⌨️ Navigasi Full Keyboard

Kasir kerja lebih cepat pakai keyboard. Semua shortcut mengikuti pola yang sudah familiar di software kasir ritel pada umumnya — bukan bikin standar baru:

| Tombol | Aksi | | Tombol | Aksi |
|---|---|---|---|---|
| `F1` | Cari produk/SKU/scan barcode | | `F7` | Tambah biaya tambahan |
| `F2` | Transaksi/pesanan baru | | `F8` | Lihat total/subtotal cepat |
| `F3` | Cari data member | | `F9` | Tulis catatan transaksi |
| `F4` | Tambah pelanggan / buka laci kas | | `F10` / `Enter` | Masuk ke pembayaran |
| `F6` | Diskon item/total | | `F11` | Toggle full screen |
| `Tab` / `Shift+Tab` | Pindah baris produk | | `Esc` | Batal / tutup pop-up |
| `↑` / `↓` | Ubah qty barang | | `Spasi` | Tampilkan/sembunyikan daftar produk |

## 🛠️ Tech Stack

```
Bahasa        → C# 14
UI Framework  → WPF (.NET 10 LTS)
Arsitektur    → MVVM
Database      → SQLite (mode WAL)
ORM           → Entity Framework Core 10
Printing      → ESC/POS raw commands (ESC-POS-.NET)
IDE           → Visual Studio Community 2026
```

## 💻 Kebutuhan Sistem

**Minimum**
- Windows 10 (64-bit) · Dual-core 1.6 GHz · RAM 2 GB · 500 MB storage

**Direkomendasikan**
- Windows 10/11 (64-bit) · Dual-core 2.0 GHz+ · RAM 4 GB+ · SSD

## 🚀 Memulai (Development)

```bash
# Clone repository
https://github.com/Bagus550/KasirKu.git
cd KasirKu

# Restore dependencies
dotnet restore

# Jalankan migrasi database
dotnet ef database update

# Jalankan aplikasi
dotnet run
```

> 💡 Butuh [.NET 10 SDK](https://dotnet.microsoft.com/download) dan Visual Studio 2026 dengan workload **.NET desktop development**. Perlu VS 2026 karena .NET 10 (target `net10.0`) tidak didukung penuh di VS 2022 — VS 2022 hanya bisa dipakai untuk downlevel targeting ke .NET 9 ke bawah.

## 🗺️ Roadmap

- [x] Perancangan database & arsitektur MVVM
- [x] Modul manajemen produk & stok
- [x] Modul transaksi kasir
- [x] Integrasi printer thermal (ESC/POS)
- [x] Modul laporan
- [x] Login & manajemen role kasir
- [x] Backup/restore database
- [ ] *Next version:* multi-cabang, sync cloud, dashboard analitik

## 📁 Struktur Data Singkat

```
Produk ──< DetailTransaksi >── Transaksi ──< Kasir
Produk ──< StokLog
```

Harga jual & harga beli disimpan per transaksi (bukan hanya rujuk ke tabel Produk), sehingga laporan laba-rugi historis tetap akurat meski harga produk berubah.

## 🤝 Kontribusi

Proyek ini masih dalam tahap belajar & pengembangan aktif. Saran, issue, atau pull request sangat terbuka — silakan buka *Issues* untuk diskusi sebelum mengirim PR besar.

## 📄 Lisensi

Didistribusikan di bawah lisensi MIT. Lihat `LICENSE` untuk detail lengkap.

---

<div align="center">
Dibuat dengan ☕ dan niat belajar desktop development.
</div>
