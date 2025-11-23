using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormDesignerAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class VersionConstructorFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentVersionId",
                table: "Forms",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "FormDefinition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FormDefinitionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormDefinition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Version",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Major = table.Column<int>(type: "INTEGER", nullable: false),
                    Minor = table.Column<int>(type: "INTEGER", nullable: false),
                    Patch = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReleasedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 4),
                    FormId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Version", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Version_FormDefinition_VersionId",
                        column: x => x.VersionId,
                        principalTable: "FormDefinition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Version_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Forms_CurrentVersionId",
                table: "Forms",
                column: "CurrentVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Version_FormId",
                table: "Version",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_Version_VersionId",
                table: "Version",
                column: "VersionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Forms_Version_CurrentVersionId",
                table: "Forms",
                column: "CurrentVersionId",
                principalTable: "Version",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Forms_Version_CurrentVersionId",
                table: "Forms");

            migrationBuilder.DropTable(
                name: "Version");

            migrationBuilder.DropTable(
                name: "FormDefinition");

            migrationBuilder.DropIndex(
                name: "IX_Forms_CurrentVersionId",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "CurrentVersionId",
                table: "Forms");
        }
    }
}
