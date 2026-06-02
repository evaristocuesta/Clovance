using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clovance.ApiService.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserInvitationRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_invitations_AspNetUsers_ConsumedByUserId",
                table: "user_invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_user_invitations_AspNetUsers_CreatedByUserId",
                table: "user_invitations");

            migrationBuilder.DropIndex(
                name: "IX_user_invitations_ConsumedByUserId",
                table: "user_invitations");

            migrationBuilder.DropIndex(
                name: "IX_user_invitations_CreatedByUserId",
                table: "user_invitations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_ConsumedByUserId",
                table: "user_invitations",
                column: "ConsumedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_invitations_CreatedByUserId",
                table: "user_invitations",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_invitations_AspNetUsers_ConsumedByUserId",
                table: "user_invitations",
                column: "ConsumedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_user_invitations_AspNetUsers_CreatedByUserId",
                table: "user_invitations",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
