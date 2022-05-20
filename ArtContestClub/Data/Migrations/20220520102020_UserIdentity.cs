using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArtContestClub.Data.Migrations
{
    public partial class UserIdentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OwnerEmail",
                table: "SubmissionComments",
                newName: "UserIdentity");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "ContestSubmissions",
                newName: "UserIdentity");

            migrationBuilder.RenameColumn(
                name: "OwnerEmail",
                table: "Contests",
                newName: "UserIdentity");

            migrationBuilder.RenameColumn(
                name: "ParticipantEmail",
                table: "ContestParticipants",
                newName: "UserIdentity");

            migrationBuilder.RenameColumn(
                name: "OwnerEmail",
                table: "ContestComments",
                newName: "UserIdentity");

            migrationBuilder.CreateTable(
                name: "AboutMe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserIdentity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fullname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutMe", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutMe");

            migrationBuilder.RenameColumn(
                name: "UserIdentity",
                table: "SubmissionComments",
                newName: "OwnerEmail");

            migrationBuilder.RenameColumn(
                name: "UserIdentity",
                table: "ContestSubmissions",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "UserIdentity",
                table: "Contests",
                newName: "OwnerEmail");

            migrationBuilder.RenameColumn(
                name: "UserIdentity",
                table: "ContestParticipants",
                newName: "ParticipantEmail");

            migrationBuilder.RenameColumn(
                name: "UserIdentity",
                table: "ContestComments",
                newName: "OwnerEmail");
        }
    }
}
