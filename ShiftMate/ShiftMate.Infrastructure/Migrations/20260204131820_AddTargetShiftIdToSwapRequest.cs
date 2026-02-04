using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftMate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTargetShiftIdToSwapRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SwapRequests_Shifts_ShiftId",
                table: "SwapRequests");

            migrationBuilder.AddColumn<Guid>(
                name: "TargetShiftId",
                table: "SwapRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SwapRequests_TargetShiftId",
                table: "SwapRequests",
                column: "TargetShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_SwapRequests_Shifts_ShiftId",
                table: "SwapRequests",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SwapRequests_Shifts_TargetShiftId",
                table: "SwapRequests",
                column: "TargetShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SwapRequests_Shifts_ShiftId",
                table: "SwapRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_SwapRequests_Shifts_TargetShiftId",
                table: "SwapRequests");

            migrationBuilder.DropIndex(
                name: "IX_SwapRequests_TargetShiftId",
                table: "SwapRequests");

            migrationBuilder.DropColumn(
                name: "TargetShiftId",
                table: "SwapRequests");

            migrationBuilder.AddForeignKey(
                name: "FK_SwapRequests_Shifts_ShiftId",
                table: "SwapRequests",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
