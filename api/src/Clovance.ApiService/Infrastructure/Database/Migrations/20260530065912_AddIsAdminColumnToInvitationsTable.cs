using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clovance.ApiService.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class AddIsAdminColumnToInvitationsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsAdmin",
            table: "user_invitations",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsAdmin",
            table: "user_invitations");
    }
}
