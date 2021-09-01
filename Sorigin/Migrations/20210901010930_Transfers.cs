using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sorigin.Migrations
{
    public partial class Transfers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transfers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transfer_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transfers", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transfers_id",
                table: "transfers",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_transfers_transfer_id",
                table: "transfers",
                column: "transfer_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transfers");
        }
    }
}
