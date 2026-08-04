using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasirKu.Migrations
{
    /// <inheritdoc />
    public partial class FixDetailTransaksiNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetailTransaksi_Produk_ProdukId",
                table: "DetailTransaksi");

            migrationBuilder.AlterColumn<int>(
                name: "ProdukId",
                table: "DetailTransaksi",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_DetailTransaksi_Produk_ProdukId",
                table: "DetailTransaksi",
                column: "ProdukId",
                principalTable: "Produk",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetailTransaksi_Produk_ProdukId",
                table: "DetailTransaksi");

            migrationBuilder.AlterColumn<int>(
                name: "ProdukId",
                table: "DetailTransaksi",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DetailTransaksi_Produk_ProdukId",
                table: "DetailTransaksi",
                column: "ProdukId",
                principalTable: "Produk",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
