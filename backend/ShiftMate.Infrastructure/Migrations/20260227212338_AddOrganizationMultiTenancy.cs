using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftMate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Skapa Organizations-tabellen först
            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name",
                unique: true);

            // 2. Skapa en standardorganisation för befintliga rader
            var defaultOrgId = Guid.NewGuid();
            migrationBuilder.Sql($@"
                INSERT INTO ""Organizations"" (""Id"", ""Name"", ""CreatedAt"")
                VALUES ('{defaultOrgId}', 'ShiftMate Demo', NOW());
            ");

            // 3. Lägg till OrganizationId-kolumner med default-värdet (standardorg)
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: defaultOrgId);

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Shifts",
                type: "uuid",
                nullable: false,
                defaultValue: defaultOrgId);

            // 4. Sätt alla befintliga rader till standardorganisationen
            migrationBuilder.Sql($@"
                UPDATE ""Users"" SET ""OrganizationId"" = '{defaultOrgId}' WHERE ""OrganizationId"" = '00000000-0000-0000-0000-000000000000';
                UPDATE ""Shifts"" SET ""OrganizationId"" = '{defaultOrgId}' WHERE ""OrganizationId"" = '00000000-0000-0000-0000-000000000000';
            ");

            // 5. Skapa index och FK-constraints
            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_OrganizationId",
                table: "Shifts",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Organizations_OrganizationId",
                table: "Shifts",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Organizations_OrganizationId",
                table: "Shifts");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_OrganizationId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Shifts");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
