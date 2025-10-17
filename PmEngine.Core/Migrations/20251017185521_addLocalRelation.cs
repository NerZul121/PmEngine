using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class addLocalRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserLocalEntity_UserId",
                table: "UserLocalEntity",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLocalEntity_UserEntity_UserId",
                table: "UserLocalEntity",
                column: "UserId",
                principalTable: "UserEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLocalEntity_UserEntity_UserId",
                table: "UserLocalEntity");

            migrationBuilder.DropIndex(
                name: "IX_UserLocalEntity_UserId",
                table: "UserLocalEntity");
        }
    }
}
