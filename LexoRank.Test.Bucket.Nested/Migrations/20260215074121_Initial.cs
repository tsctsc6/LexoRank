using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LexoRank.Test.Bucket.Nested.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LexoRankData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterSet = table.Column<string>(type: "text", nullable: false),
                    Separator = table.Column<char>(type: "character(1)", nullable: false),
                    Buckets = table.Column<string[]>(type: "text[]", nullable: false),
                    CurrentBucket = table.Column<string>(type: "text", nullable: false),
                    NextBucket = table.Column<string>(type: "text", nullable: false),
                    IsDesc = table.Column<bool>(type: "boolean", nullable: false),
                    DenominatorBase = table.Column<int>(type: "integer", nullable: false),
                    DenominatorExponent = table.Column<int>(type: "integer", nullable: false),
                    StepSizeNumerator = table.Column<string>(type: "text", nullable: false),
                    LastLexoRankValueNumerator = table.Column<string>(type: "text", nullable: false),
                    IsRebalancing = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LexoRankData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    SortingValue = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LexoRankData");

            migrationBuilder.DropTable(
                name: "Posts");
        }
    }
}
