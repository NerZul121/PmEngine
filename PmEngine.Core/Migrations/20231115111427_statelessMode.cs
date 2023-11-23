using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PmEngine.Core.Migrations
{
    /// <inheritdoc />
    public partial class statelessMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionData",
                table: "UserEntity",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionData",
                table: "UserEntity");
        }
    }
}
