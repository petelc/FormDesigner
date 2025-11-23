using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormDesignerAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCurrentVersionNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Forms_Version_CurrentVersionId",
                table: "Forms");

            migrationBuilder.DropIndex(
                name: "IX_Forms_CurrentVersionId",
                table: "Forms");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentVersionId",
                table: "Forms",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentVersionId",
                table: "Forms",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Forms_CurrentVersionId",
                table: "Forms",
                column: "CurrentVersionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Forms_Version_CurrentVersionId",
                table: "Forms",
                column: "CurrentVersionId",
                principalTable: "Version",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
