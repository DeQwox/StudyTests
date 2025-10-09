using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyTests.Migrations
{
    /// <inheritdoc />
    public partial class AddUniquePhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Teachers_PhoneNumber",
                table: "Teachers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_PhoneNumber",
                table: "Students",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_PhoneNumber",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_PhoneNumber",
                table: "Students");
        }
    }
}
