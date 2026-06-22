using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftHandoverAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitColumnsToHandover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "HandoverReports",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                table: "HandoverReports",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Unit",
                table: "HandoverReports");

            migrationBuilder.DropColumn(
                name: "UnitName",
                table: "HandoverReports");
        }
    }
}
