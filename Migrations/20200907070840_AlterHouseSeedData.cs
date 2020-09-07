using Microsoft.EntityFrameworkCore.Migrations;

namespace GroundHouse.Migrations
{
    public partial class AlterHouseSeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Houses",
                columns: new[] { "Id", "Address", "OwnerEmail", "Price", "Type" },
                values: new object[] { 2, "DefaultAddress2", "default2@gmail.com", 20000, 2 });

            migrationBuilder.InsertData(
                table: "Houses",
                columns: new[] { "Id", "Address", "OwnerEmail", "Price", "Type" },
                values: new object[] { 3, "DefaultAddress3", "default3@gmail.com", 30000, 3 });

            migrationBuilder.InsertData(
                table: "Houses",
                columns: new[] { "Id", "Address", "OwnerEmail", "Price", "Type" },
                values: new object[] { 4, "DefaultAddress4", "default4@gmail.com", 40000, 3 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Houses",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
