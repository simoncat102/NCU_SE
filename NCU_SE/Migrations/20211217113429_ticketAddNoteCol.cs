using Microsoft.EntityFrameworkCore.Migrations;

namespace NCU_SE.Migrations
{
    public partial class ticketAddNoteCol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Ticket",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
