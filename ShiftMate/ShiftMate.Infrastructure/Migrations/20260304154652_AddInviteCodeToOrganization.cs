using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftMate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteCodeToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "Organizations",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "InviteCodeGeneratedAt",
                table: "Organizations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Generate unique 8-char invite codes for existing organizations
            migrationBuilder.Sql(@"
                UPDATE ""Organizations""
                SET ""InviteCode"" = upper(substr(md5(random()::text || ""Id""::text), 1, 8)),
                    ""InviteCodeGeneratedAt"" = now()
                WHERE ""InviteCode"" = '';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_InviteCode",
                table: "Organizations",
                column: "InviteCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organizations_InviteCode",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "InviteCodeGeneratedAt",
                table: "Organizations");
        }
    }
}
