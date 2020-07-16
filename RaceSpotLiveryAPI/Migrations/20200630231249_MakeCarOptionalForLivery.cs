using Microsoft.EntityFrameworkCore.Migrations;

namespace RaceSpotLiveryAPI.Migrations
{
    public partial class MakeCarOptionalForLivery : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Liveries_Cars_CarId",
                table: "Liveries");

            migrationBuilder.AddForeignKey(
                name: "FK_Liveries_Cars_CarId",
                table: "Liveries",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Liveries_Cars_CarId",
                table: "Liveries");

            migrationBuilder.AddForeignKey(
                name: "FK_Liveries_Cars_CarId",
                table: "Liveries",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
