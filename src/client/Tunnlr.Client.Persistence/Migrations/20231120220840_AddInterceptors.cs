using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tunnlr.Client.Web.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInterceptors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Interceptors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TypeName = table.Column<string>(type: "TEXT", nullable: false),
                    Values = table.Column<string>(type: "TEXT", nullable: false),
                    TunnelIdRequestInterceptor = table.Column<Guid>(type: "TEXT", nullable: true),
                    TunnelIdResponseInterceptor = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interceptors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interceptors_Tunnels_TunnelIdRequestInterceptor",
                        column: x => x.TunnelIdRequestInterceptor,
                        principalTable: "Tunnels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Interceptors_Tunnels_TunnelIdResponseInterceptor",
                        column: x => x.TunnelIdResponseInterceptor,
                        principalTable: "Tunnels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interceptors_TunnelIdRequestInterceptor",
                table: "Interceptors",
                column: "TunnelIdRequestInterceptor");

            migrationBuilder.CreateIndex(
                name: "IX_Interceptors_TunnelIdResponseInterceptor",
                table: "Interceptors",
                column: "TunnelIdResponseInterceptor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interceptors");
        }
    }
}
