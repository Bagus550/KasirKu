# Roadmap Perbaikan Aplikasi Kasir

## 📅 Fase 1: Keamanan & Pengelolaan Sesi
**Estimasi:** 1 - 2 Hari
**Fokus:** Memastikan tidak ada kebocoran data antar-kasir/shift.

### Pembersihan Sesi (`SessionManager.cs`)
- [ ] Buat method `ClearSession()` untuk mengosongkan entitas kasir dan shift aktif.
- [ ] Integrasikan `ClearSession()` ke dalam aliran Logout dan Clock-Out.

### Pembersihan Keranjang (`KasirViewModel.cs`)
- [ ] Pastikan list `CartItems` di-`Clear()` saat sesi kasir berakhir.
- [ ] Riset/cek ulang apakah ada state unhandled saat pergantian user.

---

## 📅 Fase 2: Robustness Database & Transaksi
**Estimasi:** 2 - 3 Hari
**Fokus:** Mencegah stok korup, error konkurensi, dan masalah lifecycle EF Core.

### Database Transaction (`TransactionService.cs`)
- [ ] Bungkus proses simpan nota & pemotongan stok menggunakan `BeginTransactionAsync()`.
- [ ] Tambahkan mekanisme `RollbackAsync()` jika salah satu proses insert detail transaksi atau pemutakhiran stok gagal.

### EF Core Lifecycle (`AppDbContext.cs` & Services)
- [ ] Cek pendaftaran `DbContext` di DI Container (pastikan Transient/Scoped, bukan Singleton).
- [ ] Tangani tracking entity error pada stok produk di `ProductService.cs`.

---

## 📅 Fase 3: Safety & Hardware Handling
**Estimasi:** 1 Hari
**Fokus:** Aplikasi tetap stabil meski printer tidak tersedia/rusak.

### Graceful Degradation (`PrinterService.cs`)
- [ ] Bungkus logika pencetakan nota dengan `try-catch`.
- [ ] Tampilkan notifikasi ramah (misal: "Printer tidak terhubung, nota disimpan sebagai digital") alih-alih membuat aplikasi crash.

### Mocking untuk Testing
- [ ] Gunakan `NullPrinterService` sebagai default fallback selama tahap development/testing tanpa alat.

---

## 📅 Fase 4: Fondasi Audit Log & Finishing
**Estimasi:** 2 Hari
**Fokus:** Menyiapkan struktur pencatatan riwayat aktivitas kasir untuk masa depan.

### Fondasi Logging (`AuditLog.cs` & `LoggerService.cs`)
- [ ] Buat `IAuditLogger` untuk mencatat aksi penting (Login, Clock-In/Out, Hapus Item, Batal Transaksi).
- [ ] Pasang panggilan logger dasar di ViewModel terkait.

### Testing & Final Review
- [ ] Pengujian skenario end-to-end: Login ➔ Clock-In ➔ Transaksi ➔ Clock-Out ➔ Logout.