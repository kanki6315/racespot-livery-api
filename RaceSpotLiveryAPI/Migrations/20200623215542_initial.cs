using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RaceSpotLiveryAPI.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    IsTeam = table.Column<bool>(nullable: false),
                    IsArchived = table.Column<bool>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SeriesId = table.Column<Guid>(nullable: false),
                    RaceTime = table.Column<DateTime>(nullable: false),
                    BroadcastLink = table.Column<string>(nullable: true),
                    EventState = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Liveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LiveryType = table.Column<int>(nullable: false),
                    SeriesId = table.Column<Guid>(nullable: false),
                    ITeamId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Liveries_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_SeriesId",
                table: "Events",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Liveries_SeriesId",
                table: "Liveries",
                column: "SeriesId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Liveries");

            migrationBuilder.DropTable(
                name: "Series");
        }
    }
}
