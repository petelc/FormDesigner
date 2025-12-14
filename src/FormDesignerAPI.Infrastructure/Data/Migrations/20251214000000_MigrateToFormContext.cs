using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormDesignerAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToFormContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old Forms table and recreate with FormContext schema
            migrationBuilder.DropTable(
                name: "Forms");

            // Create new Forms table with FormContext schema
            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DefinitionSchema = table.Column<string>(type: "TEXT", nullable: false),
                    Origin_Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Origin_CreatedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Origin_CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    Origin_ReferenceId = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.Id);
                });

            // Create FormRevisions table
            migrationBuilder.CreateTable(
                name: "FormRevisions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FormId = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    DefinitionSchema = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormRevisions_Forms_FormId",
                        column: x => x.FormId,
                        principalTable: "Forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes on Forms
            migrationBuilder.CreateIndex(
                name: "IX_Forms_Name",
                table: "Forms",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Forms_IsActive",
                table: "Forms",
                column: "IsActive");

            // Create indexes on FormRevisions
            migrationBuilder.CreateIndex(
                name: "IX_FormRevisions_FormId",
                table: "FormRevisions",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormRevisions_Version",
                table: "FormRevisions",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_FormRevisions_FormId_Version",
                table: "FormRevisions",
                columns: new[] { "FormId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FormContext tables
            migrationBuilder.DropTable(
                name: "FormRevisions");

            migrationBuilder.DropTable(
                name: "Forms");

            // Recreate old Forms table (FormAggregate schema)
            migrationBuilder.CreateTable(
                name: "Forms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FormNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FormTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Division = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Owner_Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Owner_Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<string>(type: "TEXT", nullable: false),
                    RevisedDate = table.Column<string>(type: "TEXT", nullable: false),
                    ConfigurationPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forms", x => x.Id);
                });
        }
    }
}
