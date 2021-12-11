using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NCU_SE.Migrations
{
    public partial class AddTicketTable3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    TicketID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    FlightID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepartureDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArriveDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartAirport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DestinationAirport = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.TicketID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ticket");
        }
    }
}
