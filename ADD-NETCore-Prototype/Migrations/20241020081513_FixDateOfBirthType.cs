using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ADD_NETCore_Prototype.Migrations
{
    /// <inheritdoc />
    public partial class FixDateOfBirthType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "Pets",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DateOfBirth",
                table: "Pets",
                type: "int",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2"
            );
        }
    }
}
