using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NCU_SE.Migrations
{
    public partial class addFlightToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Flight",
                columns: table => new
                {
                    FlightID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FlightCode = table.Column<int>(type: "int", nullable: false),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    Airline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CityTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CityFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirportTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirportFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DepDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArriDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArriTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FlightNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flight", x => x.FlightID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Flight");
        }
    }
}
