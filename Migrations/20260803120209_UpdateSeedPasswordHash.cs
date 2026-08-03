using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KasirKu.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kasir",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEO0srbQ83jUrJaGs7LXC/T4XBo4K0PI7tP+OoGJejP6W0lq58QnE0kSeaBXeKMxztw==");

            migrationBuilder.UpdateData(
                table: "Kasir",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEOeir4tK5zQjyy/KQbzKV4ccC5RwGF/rpp1ODlwWGg7bbImiZhL2O9OdDsaj05pJOw==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kasir",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEG3c1P5S3V9R4Y8S+X9X+k+Q3m8eJ/9J5rX9dJ9L2k1m8n1o2p3q4r5s6t7u8v==");

            migrationBuilder.UpdateData(
                table: "Kasir",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEK4d2Q6T4W0S5Z9T+Y0Y+l+R4n9fK/0K6sY0eK0M3l2n9o2p3q4r5s6t7u8w==");
        }
    }
}
