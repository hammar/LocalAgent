using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalAgent.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToAgent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Agents",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Agents");
        }
    }
}
