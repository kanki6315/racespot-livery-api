using Microsoft.EntityFrameworkCore.Migrations;

namespace RaceSpotLiveryAPI.Migrations
{
    public partial class AddLogoUrlToSeries : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Series",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoImgUrl",
                table: "Series",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "LogoImgUrl",
                table: "Series");
        }
    }
}
