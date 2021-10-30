using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace Sorigin.Migrations
{
    public partial class LocalMedia : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    path = table.Column<string>(type: "text", nullable: false),
                    mime_type = table.Column<string>(type: "text", nullable: false),
                    file_hash = table.Column<string>(type: "text", nullable: false),
                    contract = table.Column<string>(type: "text", nullable: false),
                    uploaded = table.Column<Instant>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_media", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_media_contract",
                table: "media",
                column: "contract");

            migrationBuilder.CreateIndex(
                name: "ix_media_file_hash",
                table: "media",
                column: "file_hash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media");
        }
    }
}
