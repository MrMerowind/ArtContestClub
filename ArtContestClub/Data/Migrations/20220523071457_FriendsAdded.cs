using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtContestClub.Data.Migrations
{
    public partial class FriendsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserIdentity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FriendIdentity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeAdded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Friends");
        }
    }
}
