using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clovance.ApiService.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddInvitationsFlow : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "MustCompleteOnboarding",
            table: "AspNetUsers",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
            name: "user_invitations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                TokenHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                ConsumedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_invitations", x => x.Id);
                table.ForeignKey(
                    name: "FK_user_invitations_AspNetUsers_ConsumedByUserId",
                    column: x => x.ConsumedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_user_invitations_AspNetUsers_CreatedByUserId",
                    column: x => x.CreatedByUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_user_invitations_ConsumedByUserId",
            table: "user_invitations",
            column: "ConsumedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_user_invitations_CreatedByUserId",
            table: "user_invitations",
            column: "CreatedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_user_invitations_Email",
            table: "user_invitations",
            column: "Email");

        migrationBuilder.CreateIndex(
            name: "IX_user_invitations_ExpiresAt",
            table: "user_invitations",
            column: "ExpiresAt");

        migrationBuilder.CreateIndex(
            name: "IX_user_invitations_TokenHash",
            table: "user_invitations",
            column: "TokenHash",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "user_invitations");

        migrationBuilder.DropColumn(
            name: "MustCompleteOnboarding",
            table: "AspNetUsers");
    }
}
