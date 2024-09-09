using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackingSheet.Migrations
{
    /// <inheritdoc />
    public partial class KanbanMigrosBoardCreating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "KanbanBoards",
                columns: new[] { "Id", "Board", "CreatedAt" },
                values: new object[] { new Guid("d6b8e802-6fc0-4765-a1b9-2ee559cd1500"), "Default Board", new DateTime(2024, 9, 8, 13, 9, 1, 290, DateTimeKind.Utc).AddTicks(8740) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "KanbanBoards",
                keyColumn: "Id",
                keyValue: new Guid("d6b8e802-6fc0-4765-a1b9-2ee559cd1500"));
        }
    }
}
