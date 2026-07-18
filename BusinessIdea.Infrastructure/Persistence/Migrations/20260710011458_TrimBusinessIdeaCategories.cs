using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessIdea.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TrimBusinessIdeaCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""BusinessIdeas""
                SET ""Categories"" = (
                    SELECT array_agg(DISTINCT mapped)
                    FROM (
                        SELECT CASE cat
                            WHEN 1  THEN 0
                            WHEN 8  THEN 7
                            WHEN 10 THEN 4
                            WHEN 11 THEN 0
                            WHEN 12 THEN 7
                            WHEN 13 THEN 2
                            ELSE cat
                        END AS mapped
                        FROM unnest(""Categories"") AS cat
                    ) AS remapped
                )
                WHERE ""Categories"" && ARRAY[1, 8, 10, 11, 12, 13]::smallint[];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty: this remap is lossy (e.g. Agriculture and Tech
            // both collapse toward existing categories), so original per-idea
            // category selections can't be reconstructed on rollback.
        }
    }
}