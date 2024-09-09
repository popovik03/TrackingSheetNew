using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackingSheet.Migrations
{
    /// <inheritdoc />
    public partial class NewKanbanMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "KanbanBoards",
                keyColumn: "Id",
                keyValue: new Guid("d6b8e802-6fc0-4765-a1b9-2ee559cd1500"));

            migrationBuilder.AddColumn<bool>(
                name: "IsProtected",
                table: "KanbanBoards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "KanbanBoards",
                columns: new[] { "Id", "Board", "CreatedAt", "IsProtected" },
                values: new object[] { new Guid("2b2c642c-0ec6-4962-99ea-27097ea6c6fe"), "Default Board", new DateTime(2024, 9, 9, 4, 21, 37, 978, DateTimeKind.Utc).AddTicks(3041), true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "KanbanBoards",
                keyColumn: "Id",
                keyValue: new Guid("2b2c642c-0ec6-4962-99ea-27097ea6c6fe"));

            migrationBuilder.DropColumn(
                name: "IsProtected",
                table: "KanbanBoards");

            migrationBuilder.InsertData(
                table: "KanbanBoards",
                columns: new[] { "Id", "Board", "CreatedAt" },
                values: new object[] { new Guid("d6b8e802-6fc0-4765-a1b9-2ee559cd1500"), "Default Board", new DateTime(2024, 9, 8, 13, 9, 1, 290, DateTimeKind.Utc).AddTicks(8740) });
        }
    }
}
