using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tunnlr.Client.Web.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnabledStateToInterceptor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Enabled",
                table: "Interceptors",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Enabled",
                table: "Interceptors");
        }
    }
}
