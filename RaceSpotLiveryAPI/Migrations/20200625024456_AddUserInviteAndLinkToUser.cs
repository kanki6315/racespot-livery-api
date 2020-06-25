using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RaceSpotLiveryAPI.Migrations
{
    public partial class AddUserInviteAndLinkToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    IracingId = table.Column<string>(nullable: false),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInvites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInvites_UserId",
                table: "UserInvites",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInvites");
        }
    }
}
