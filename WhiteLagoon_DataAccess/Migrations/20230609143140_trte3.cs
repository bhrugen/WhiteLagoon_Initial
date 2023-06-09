using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WhiteLagoon_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class trte3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "VillaNumbers",
                columns: new[] { "Villa_Number", "SpecialDetails", "VillaId" },
                values: new object[,]
                {
                    { 101, null, 1 },
                    { 102, null, 1 },
                    { 103, null, 1 },
                    { 104, null, 2 },
                    { 105, null, 2 },
                    { 106, null, 2 },
                    { 107, null, 3 },
                    { 108, null, 3 },
                    { 109, null, 3 },
                    { 110, null, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "VillaNumbers",
                keyColumn: "Villa_Number",
                keyValue: 110);
        }
    }
}
