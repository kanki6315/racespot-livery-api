using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RaceSpotLiveryAPI.Migrations
{
    public partial class AddRejectionNoticeEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRejected",
                table: "Liveries",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Rejections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Message = table.Column<string>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    LiveryId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rejections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rejections_Liveries_LiveryId",
                        column: x => x.LiveryId,
                        principalTable: "Liveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rejections_LiveryId",
                table: "Rejections",
                column: "LiveryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rejections");

            migrationBuilder.DropColumn(
                name: "IsRejected",
                table: "Liveries");
        }
    }
}
