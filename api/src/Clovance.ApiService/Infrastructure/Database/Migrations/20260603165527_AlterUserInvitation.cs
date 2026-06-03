using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clovance.ApiService.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AlterUserInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "user_invitations");

            migrationBuilder.RenameColumn(
                name: "ConsumedByUserId",
                table: "user_invitations",
                newName: "ConsumedBy");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "user_invitations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedAt",
                table: "user_invitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "user_invitations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "user_invitations");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "user_invitations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "user_invitations");

            migrationBuilder.RenameColumn(
                name: "ConsumedBy",
                table: "user_invitations",
                newName: "ConsumedByUserId");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "user_invitations",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");
        }
    }
}
