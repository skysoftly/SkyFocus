using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyFocus.Migrations
{
    /// <inheritdoc />
    public partial class AddNoteText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NoteText",
                table: "App",
                type: "TEXT",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NoteText",
                table: "App");
        }
    }
}
