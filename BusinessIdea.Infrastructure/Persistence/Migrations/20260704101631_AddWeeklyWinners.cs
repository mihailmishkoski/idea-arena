using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessIdea.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyWinners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeeklyWinners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStartUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PeriodEndUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PostId = table.Column<Guid>(type: "uuid", nullable: true),
                    PostName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AuthorId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UpVotes = table.Column<int>(type: "integer", nullable: false),
                    DownVotes = table.Column<int>(type: "integer", nullable: false),
                    CommentCount = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyWinners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyWinners_BusinessIdeas_PostId",
                        column: x => x.PostId,
                        principalTable: "BusinessIdeas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyWinners_PeriodStartUtc",
                table: "WeeklyWinners",
                column: "PeriodStartUtc",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyWinners_PostId",
                table: "WeeklyWinners",
                column: "PostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeeklyWinners");
        }
    }
}
