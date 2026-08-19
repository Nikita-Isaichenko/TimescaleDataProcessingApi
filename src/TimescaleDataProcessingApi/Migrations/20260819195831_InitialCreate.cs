using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TimescaleDataProcessingApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    DeltaTimeSeconds = table.Column<double>(type: "double precision", nullable: false),
                    FirstOperationStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvgExecutionTime = table.Column<double>(type: "double precision", nullable: false),
                    AvgValue = table.Column<double>(type: "double precision", nullable: false),
                    MedianValue = table.Column<double>(type: "double precision", nullable: false),
                    MaxValue = table.Column<double>(type: "double precision", nullable: false),
                    MinValue = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "values",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutionTime = table.Column<double>(type: "double precision", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ResultId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_values_results_ResultId",
                        column: x => x.ResultId,
                        principalTable: "results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_values_ResultId",
                table: "values",
                column: "ResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "values");

            migrationBuilder.DropTable(
                name: "results");
        }
    }
}
