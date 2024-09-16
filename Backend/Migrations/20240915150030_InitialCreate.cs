using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Prizes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromotionalGames",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionalGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonetaryPrizes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MonetaryValue = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonetaryPrizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonetaryPrizes_Prizes_Id",
                        column: x => x.Id,
                        principalTable: "Prizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NonMonetaryPrizes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonMonetaryPrizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NonMonetaryPrizes_Prizes_Id",
                        column: x => x.Id,
                        principalTable: "Prizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GridElements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Index = table.Column<int>(type: "INTEGER", nullable: false),
                    HasBeenFlipped = table.Column<bool>(type: "INTEGER", nullable: false),
                    PrizeId = table.Column<string>(type: "TEXT", nullable: true),
                    GridPromotionalGameId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GridElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GridElements_Prizes_PrizeId",
                        column: x => x.PrizeId,
                        principalTable: "Prizes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GridElements_PromotionalGames_GridPromotionalGameId",
                        column: x => x.GridPromotionalGameId,
                        principalTable: "PromotionalGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GridElements_GridPromotionalGameId",
                table: "GridElements",
                column: "GridPromotionalGameId");

            migrationBuilder.CreateIndex(
                name: "IX_GridElements_PrizeId",
                table: "GridElements",
                column: "PrizeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GridElements");

            migrationBuilder.DropTable(
                name: "MonetaryPrizes");

            migrationBuilder.DropTable(
                name: "NonMonetaryPrizes");

            migrationBuilder.DropTable(
                name: "PromotionalGames");

            migrationBuilder.DropTable(
                name: "Prizes");
        }
    }
}
