using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhiteLagoon_DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addAmenity12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Amenities",
                newName: "AmenityName");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Amenities",
                newName: "AmenityDescription");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AmenityName",
                table: "Amenities",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "AmenityDescription",
                table: "Amenities",
                newName: "Description");
        }
    }
}
