using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasirKu.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedAkun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kasir",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Nama", "Username" },
                values: new object[] { "Admin KasirKu", "Admin" });

            migrationBuilder.UpdateData(
                table: "Kasir",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Nama", "Username" },
                values: new object[] { "Kasir", "Kasir" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kasir",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Nama", "Username" },
                values: new object[] { "Bagus Setiawan", "Bagus" });

            migrationBuilder.UpdateData(
                table: "Kasir",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Nama", "Username" },
                values: new object[] { "Anton Wijaya", "Anton" });
        }
    }
}
