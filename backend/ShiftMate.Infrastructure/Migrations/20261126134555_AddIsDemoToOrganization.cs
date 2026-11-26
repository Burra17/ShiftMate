using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftMate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDemoToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDemo",
                table: "Organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDemo",
                table: "Organizations");
        }
    }
}
