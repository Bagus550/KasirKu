using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KasirKu.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kasir",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nama = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kasir", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produk",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SKU = table.Column<string>(type: "TEXT", nullable: true),
                    Nama = table.Column<string>(type: "TEXT", nullable: false),
                    Kategori = table.Column<string>(type: "TEXT", nullable: true),
                    HargaBeli = table.Column<decimal>(type: "TEXT", nullable: false),
                    HargaJual = table.Column<decimal>(type: "TEXT", nullable: false),
                    Stok = table.Column<int>(type: "INTEGER", nullable: false),
                    StokMinimum = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produk", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shift",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NamaShift = table.Column<string>(type: "TEXT", nullable: false),
                    JamMulai = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    JamSelesai = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    IsAktif = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shift", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KasirId = table.Column<int>(type: "INTEGER", nullable: false),
                    Waktu = table.Column<DateTime>(type: "TEXT", nullable: false),
                    JenisAksi = table.Column<string>(type: "TEXT", nullable: false),
                    Keterangan = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLog_Kasir_KasirId",
                        column: x => x.KasirId,
                        principalTable: "Kasir",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KasirSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KasirId = table.Column<int>(type: "INTEGER", nullable: false),
                    ShiftId = table.Column<int>(type: "INTEGER", nullable: true),
                    WaktuLogin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WaktuLogout = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModalAwal = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalTunaiSistem = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalTunaiFisik = table.Column<decimal>(type: "TEXT", nullable: false),
                    SelisihKas = table.Column<decimal>(type: "TEXT", nullable: false),
                    CatatanSelisih = table.Column<string>(type: "TEXT", nullable: true),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KasirSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KasirSession_Kasir_KasirId",
                        column: x => x.KasirId,
                        principalTable: "Kasir",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KasirSession_Shift_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shift",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transaksi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomorNota = table.Column<string>(type: "TEXT", nullable: false),
                    Tanggal = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalHarga = table.Column<decimal>(type: "TEXT", nullable: false),
                    TotalBayar = table.Column<decimal>(type: "TEXT", nullable: false),
                    Kembalian = table.Column<decimal>(type: "TEXT", nullable: false),
                    KasirSessionId = table.Column<int>(type: "INTEGER", nullable: true),
                    NamaKasir = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaksi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transaksi_KasirSession_KasirSessionId",
                        column: x => x.KasirSessionId,
                        principalTable: "KasirSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetailTransaksi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransaksiId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProdukId = table.Column<int>(type: "INTEGER", nullable: false),
                    NamaProduk = table.Column<string>(type: "TEXT", nullable: false),
                    HargaJual = table.Column<decimal>(type: "TEXT", nullable: false),
                    Jumlah = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetailTransaksi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetailTransaksi_Produk_ProdukId",
                        column: x => x.ProdukId,
                        principalTable: "Produk",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetailTransaksi_Transaksi_TransaksiId",
                        column: x => x.TransaksiId,
                        principalTable: "Transaksi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Kasir",
                columns: new[] { "Id", "Nama", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, "Bagus Setiawan", "AQAAAAIAAYagAAAAEG3c1P5S3V9R4Y8S+X9X+k+Q3m8eJ/9J5rX9dJ9L2k1m8n1o2p3q4r5s6t7u8v==", "Admin", "Bagus" },
                    { 2, "Anton Wijaya", "AQAAAAIAAYagAAAAEK4d2Q6T4W0S5Z9T+Y0Y+l+R4n9fK/0K6sY0eK0M3l2n9o2p3q4r5s6t7u8w==", "Kasir", "Anton" }
                });

            migrationBuilder.InsertData(
                table: "Produk",
                columns: new[] { "Id", "HargaBeli", "HargaJual", "Kategori", "Nama", "SKU", "Stok", "StokMinimum" },
                values: new object[,]
                {
                    { 1, 60000m, 68000m, "Sembako", "Beras 5kg", "BRS01", 20, 5 },
                    { 2, 14000m, 16000m, "Sembako", "Minyak Goreng 1L", "MYK01", 30, 10 },
                    { 3, 10000m, 12000m, "Sembako", "Tepung Terigu 1kg", "TPG01", 30, 5 },
                    { 4, 14000m, 15000m, "Sembako", "Tepung Tapioka 1Kg", "TPG02", 30, 10 },
                    { 5, 3000m, 3500m, "Makanan Instan", "Indomie Goreng 1 Bks", "IND01", 80, 10 },
                    { 6, 3000m, 3500m, "Makanan Instan", "Indomie Kuah Ayam Bawang 1 Bks", "IND02", 80, 10 },
                    { 7, 3000m, 3500m, "Makanan Instan", "Mie Sedaap Soto 1 Bks", "SDP01", 80, 10 },
                    { 8, 2000m, 2500m, "Penyedap", "Garam Daun 250g", "GRM01", 30, 5 },
                    { 9, 13200m, 14000m, "Penyedap", "Sasa Penyedap Rasa 250g", "SSA01", 50, 10 }
                });

            migrationBuilder.InsertData(
                table: "Shift",
                columns: new[] { "Id", "IsAktif", "JamMulai", "JamSelesai", "NamaShift" },
                values: new object[,]
                {
                    { 1, true, new TimeSpan(0, 6, 0, 0, 0), new TimeSpan(0, 12, 0, 0, 0), "Shift Pagi" },
                    { 2, true, new TimeSpan(0, 12, 0, 0, 0), new TimeSpan(0, 17, 0, 0, 0), "Shift Siang" },
                    { 3, true, new TimeSpan(0, 17, 0, 0, 0), new TimeSpan(0, 22, 0, 0, 0), "Shift Sore/Malam" },
                    { 4, false, new TimeSpan(0, 22, 0, 0, 0), new TimeSpan(0, 6, 0, 0, 0), "Shift Malam (24 Jam)" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLog_KasirId",
                table: "AuditLog",
                column: "KasirId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailTransaksi_ProdukId",
                table: "DetailTransaksi",
                column: "ProdukId");

            migrationBuilder.CreateIndex(
                name: "IX_DetailTransaksi_TransaksiId",
                table: "DetailTransaksi",
                column: "TransaksiId");

            migrationBuilder.CreateIndex(
                name: "IX_KasirSession_KasirId",
                table: "KasirSession",
                column: "KasirId");

            migrationBuilder.CreateIndex(
                name: "IX_KasirSession_ShiftId",
                table: "KasirSession",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaksi_KasirSessionId",
                table: "Transaksi",
                column: "KasirSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLog");

            migrationBuilder.DropTable(
                name: "DetailTransaksi");

            migrationBuilder.DropTable(
                name: "Produk");

            migrationBuilder.DropTable(
                name: "Transaksi");

            migrationBuilder.DropTable(
                name: "KasirSession");

            migrationBuilder.DropTable(
                name: "Kasir");

            migrationBuilder.DropTable(
                name: "Shift");
        }
    }
}
