using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class InitCreatee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Rooms_RoomId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_Softwares_Rooms_RoomId",
                table: "Softwares");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "Softwares",
                newName: "LabId");

            migrationBuilder.RenameIndex(
                name: "IX_Softwares_RoomId",
                table: "Softwares",
                newName: "IX_Softwares_LabId");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "Devices",
                newName: "LabId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_RoomId",
                table: "Devices",
                newName: "IX_Devices_LabId");

            migrationBuilder.CreateTable(
                name: "Labs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labs", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$Zpa4wUPRvXt.6HU.adAh7eS1sW5HLPdKhizajPwA8uWJDT1TTLJN6");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Labs_LabId",
                table: "Devices",
                column: "LabId",
                principalTable: "Labs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Softwares_Labs_LabId",
                table: "Softwares",
                column: "LabId",
                principalTable: "Labs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Devices_Labs_LabId",
                table: "Devices");

            migrationBuilder.DropForeignKey(
                name: "FK_Softwares_Labs_LabId",
                table: "Softwares");

            migrationBuilder.DropTable(
                name: "Labs");

            migrationBuilder.RenameColumn(
                name: "LabId",
                table: "Softwares",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Softwares_LabId",
                table: "Softwares",
                newName: "IX_Softwares_RoomId");

            migrationBuilder.RenameColumn(
                name: "LabId",
                table: "Devices",
                newName: "RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Devices_LabId",
                table: "Devices",
                newName: "IX_Devices_RoomId");

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$12$IB2NMH.XZZ3JHpfICQfpJ.jr3p/uLT0/CV3oElpInzmxrc7ttBZCi");

            migrationBuilder.AddForeignKey(
                name: "FK_Devices_Rooms_RoomId",
                table: "Devices",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Softwares_Rooms_RoomId",
                table: "Softwares",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }
    }
}
