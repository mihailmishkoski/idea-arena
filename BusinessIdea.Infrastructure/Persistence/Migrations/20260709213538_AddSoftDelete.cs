using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessIdea.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "WeeklyWinners",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "PostVotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "CommentVotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Comments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "ChatMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "BusinessIdeas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "WeeklyWinners");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "PostVotes");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "CommentVotes");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "BusinessIdeas");
        }
    }
}
