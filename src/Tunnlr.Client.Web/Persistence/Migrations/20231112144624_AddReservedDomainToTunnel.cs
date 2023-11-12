using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tunnlr.Client.Web.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservedDomainToTunnel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReservedDomains",
                columns: table => new
                {
                    Domain = table.Column<string>(type: "TEXT", nullable: false),
                    TunnelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservedDomains", x => x.Domain);
                    table.ForeignKey(
                        name: "FK_ReservedDomains_Tunnels_TunnelId",
                        column: x => x.TunnelId,
                        principalTable: "Tunnels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservedDomains_TunnelId",
                table: "ReservedDomains",
                column: "TunnelId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservedDomains");
        }
    }
}
