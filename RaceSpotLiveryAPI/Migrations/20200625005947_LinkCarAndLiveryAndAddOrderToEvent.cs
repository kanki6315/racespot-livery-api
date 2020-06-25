using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RaceSpotLiveryAPI.Migrations
{
    public partial class LinkCarAndLiveryAndAddOrderToEvent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Liveries_AspNetUsers_UserId1",
                table: "Liveries");

            migrationBuilder.DropIndex(
                name: "IX_Liveries_UserId1",
                table: "Liveries");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Liveries");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Liveries",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CarId",
                table: "Liveries",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ITeamName",
                table: "Liveries",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Events",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Liveries_CarId",
                table: "Liveries",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Liveries_UserId",
                table: "Liveries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Liveries_Cars_CarId",
                table: "Liveries",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Liveries_AspNetUsers_UserId",
                table: "Liveries",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Liveries_Cars_CarId",
                table: "Liveries");

            migrationBuilder.DropForeignKey(
                name: "FK_Liveries_AspNetUsers_UserId",
                table: "Liveries");

            migrationBuilder.DropIndex(
                name: "IX_Liveries_CarId",
                table: "Liveries");

            migrationBuilder.DropIndex(
                name: "IX_Liveries_UserId",
                table: "Liveries");

            migrationBuilder.DropColumn(
                name: "CarId",
                table: "Liveries");

            migrationBuilder.DropColumn(
                name: "ITeamName",
                table: "Liveries");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Events");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Liveries",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Liveries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Liveries_UserId1",
                table: "Liveries",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Liveries_AspNetUsers_UserId1",
                table: "Liveries",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
