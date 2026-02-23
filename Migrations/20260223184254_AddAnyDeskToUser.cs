using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkTicketManager.Migrations
{
    /// <inheritdoc />
    public partial class AddAnyDeskToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnyDesk",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnyDesk",
                table: "Users");
        }
    }
}
