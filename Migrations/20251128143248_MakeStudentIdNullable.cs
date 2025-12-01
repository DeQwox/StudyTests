using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyTests.Migrations
{
    /// <inheritdoc />
    public partial class MakeStudentIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PassedTests_AspNetUsers_StudentId",
                table: "PassedTests");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "PassedTests",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_PassedTests_AspNetUsers_StudentId",
                table: "PassedTests",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PassedTests_AspNetUsers_StudentId",
                table: "PassedTests");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "PassedTests",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PassedTests_AspNetUsers_StudentId",
                table: "PassedTests",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
