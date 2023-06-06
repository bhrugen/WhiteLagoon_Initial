using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhiteLagoon_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addAmenity1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AmenityId",
                table: "Amenities",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Amenities",
                newName: "AmenityId");
        }
    }
}
