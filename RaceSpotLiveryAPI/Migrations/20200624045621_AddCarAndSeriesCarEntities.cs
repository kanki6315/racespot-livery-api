using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RaceSpotLiveryAPI.Migrations
{
    public partial class AddCarAndSeriesCarEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Path = table.Column<string>(nullable: false),
                    LogoImgUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeriesCars",
                columns: table => new
                {
                    SeriesId = table.Column<Guid>(nullable: false),
                    CarId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesCars", x => new { x.SeriesId, x.CarId });
                    table.ForeignKey(
                        name: "FK_SeriesCars_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeriesCars_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SeriesCars_CarId",
                table: "SeriesCars",
                column: "CarId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeriesCars");

            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
