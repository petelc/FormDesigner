using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FormDesignerAPI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewFormMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigurationPath",
                table: "Forms");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Forms");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Forms",
                type: "INTEGER",
                nullable: false,
                defaultValue: 4,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Forms",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<Guid>(
                name: "FormId",
                table: "Forms",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormId",
                table: "Forms");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Forms",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldDefaultValue: 4);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Forms",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "ConfigurationPath",
                table: "Forms",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "Forms",
                type: "TEXT",
                maxLength: 8,
                nullable: true);
        }
    }
}
