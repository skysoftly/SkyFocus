using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyFocus.Migrations
{
    /// <inheritdoc />
    public partial class SetNameUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_App_Path",
                table: "App",
                column: "Path",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_App_Path",
                table: "App");
        }
    }
}
