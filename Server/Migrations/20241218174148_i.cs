using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class i : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Rooms_RoomId",
                table: "Devices");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Devices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Devices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$oQ0Ut2pLEWgH70dYUci50.vGxrXzPyVIN8BC5tYReLoawUyWpL9iu");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Rooms_RoomId",
                table: "Devices",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Rooms_RoomId",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Devices");

            migrationBuilder.AlterColumn<int>(
                name: "RoomId",
                table: "Devices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$Xkh/vhE02gb1qfkLXLNJ4ObgIGjTG8.6a1FfrxZcEkTbYP1dOTM62");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Rooms_RoomId",
                table: "Devices",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
