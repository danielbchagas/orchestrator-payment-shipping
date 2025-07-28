using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Samples.Orchestrator.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Error",
                table: "OrderStates");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "OrderStates");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "OrderStates");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentState",
                table: "OrderStates",
                type: "VARCHAR",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "VARCHAR",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Payload",
                table: "OrderStates",
                type: "VARCHAR",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payload",
                table: "OrderStates");

            migrationBuilder.AlterColumn<string>(
                name: "CurrentState",
                table: "OrderStates",
                type: "VARCHAR",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "VARCHAR",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "OrderStates",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderId",
                table: "OrderStates",
                type: "INT",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "OrderStates",
                type: "VARCHAR",
                maxLength: 100,
                nullable: true);
        }
    }
}
