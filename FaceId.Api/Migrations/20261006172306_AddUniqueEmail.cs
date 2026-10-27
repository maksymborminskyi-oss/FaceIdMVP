using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaceId.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_app_user_email",
                table: "app_user",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />     
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_app_user_emaiлl",
                table: "app_user");
        }
    }
}
