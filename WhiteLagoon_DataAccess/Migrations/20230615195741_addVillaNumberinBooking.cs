using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhiteLagoon_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addVillaNumberinBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VillaNumber",
                table: "BookingDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VillaNumber",
                table: "BookingDetails");
        }
    }
}
