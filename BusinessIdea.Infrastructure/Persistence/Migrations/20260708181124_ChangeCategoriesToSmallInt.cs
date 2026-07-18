using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessIdea.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCategoriesToSmallInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte[]>(
                name: "Categories",
                table: "BusinessIdeas",
                type: "smallint[]",
                nullable: false,
                oldClrType: typeof(int[]),
                oldType: "integer[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int[]>(
                name: "Categories",
                table: "BusinessIdeas",
                type: "integer[]",
                nullable: false,
                oldClrType: typeof(byte[]),
                oldType: "smallint[]");
        }
    }
}
