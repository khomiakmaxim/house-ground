using Microsoft.EntityFrameworkCore.Migrations;

namespace GroundHouse.Migrations
{
    public partial class SeedHousesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Houses",
                columns: new[] { "Id", "Address", "OwnerEmail", "Price", "Type" },
                values: new object[] { 1, "DefaultAddress", "default@gmail.com", 10000, 1 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
