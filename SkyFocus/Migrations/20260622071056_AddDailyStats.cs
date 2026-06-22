using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkyFocus.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyAppStat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsageTimeSeconds = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAppStat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyAppStat_App_AppId",
                        column: x => x.AppId,
                        principalTable: "App",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_App_IsFavorite",
                table: "App",
                column: "IsFavorite");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAppStat_AppId_Date",
                table: "DailyAppStat",
                columns: new[] { "AppId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyAppStat");

            migrationBuilder.DropIndex(
                name: "IX_App_IsFavorite",
                table: "App");
        }
    }
}
