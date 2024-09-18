using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class RecordFlipper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Timestamp",
                table: "GridElements",
                type: "INTEGER",
                rowVersion: true,
                nullable: false,
                defaultValueSql: "0",
                oldClrType: typeof(byte[]),
                oldType: "BLOB",
                oldRowVersion: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Flipper",
                table: "GridElements",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Flipper",
                table: "GridElements");

            migrationBuilder.AlterColumn<byte[]>(
                name: "Timestamp",
                table: "GridElements",
                type: "BLOB",
                rowVersion: true,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldRowVersion: true,
                oldDefaultValueSql: "0");
        }
    }
}
