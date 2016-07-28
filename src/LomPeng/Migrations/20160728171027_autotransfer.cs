using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LomPeng.Migrations
{
    public partial class autotransfer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoTransferSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGeneratedOnAdd", true),
                    AutoTransferAmount = table.Column<double>(nullable: false),
                    AutoTransferFirstPayment = table.Column<DateTime>(nullable: false),
                    AutoTransferIntervalInHours = table.Column<int>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    LastUpdate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoTransferSettings", x => x.Id);
                });

            migrationBuilder.AddColumn<int>(
                name: "AutoTransferId",
                table: "ChildAccounts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChildAccounts_AutoTransferId",
                table: "ChildAccounts",
                column: "AutoTransferId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChildAccounts_AutoTransferSettings_AutoTransferId",
                table: "ChildAccounts",
                column: "AutoTransferId",
                principalTable: "AutoTransferSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChildAccounts_AutoTransferSettings_AutoTransferId",
                table: "ChildAccounts");

            migrationBuilder.DropIndex(
                name: "IX_ChildAccounts_AutoTransferId",
                table: "ChildAccounts");

            migrationBuilder.DropColumn(
                name: "AutoTransferId",
                table: "ChildAccounts");

            migrationBuilder.DropTable(
                name: "AutoTransferSettings");
        }
    }
}
