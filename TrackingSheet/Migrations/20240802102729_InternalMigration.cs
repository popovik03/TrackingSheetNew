using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackingSheet.Migrations
{
    /// <inheritdoc />
    public partial class InternalMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncidentList",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Shift = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reporter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VSAT = table.Column<int>(type: "int", nullable: false),
                    Well = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Run = table.Column<int>(type: "int", nullable: false),
                    SavedNPT = table.Column<long>(type: "bigint", nullable: false),
                    ProblemType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HighLight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Solution = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentList", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ROemployees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROemployees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeePlaner2024",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    January = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    February = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    March = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    April = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    May = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    June = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    July = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    August = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    September = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    October = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    November = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    December = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePlaner2024", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeePlaner2024_ROemployees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "ROemployees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePlaner2024_EmployeeId",
                table: "EmployeePlaner2024",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeePlaner2024");

            migrationBuilder.DropTable(
                name: "IncidentList");

            migrationBuilder.DropTable(
                name: "ROemployees");
        }
    }
}
