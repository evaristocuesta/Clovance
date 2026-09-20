using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clovance.ApiService.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class ChangeColumnTokenToTokenHash : Migration
{
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM refresh_tokens");
            migrationBuilder.Sql("DELETE FROM user_invitations");

            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_token",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "token",
                table: "refresh_tokens");

            migrationBuilder.AlterColumn<string>(
                name: "token_hash",
                table: "user_invitations",
                type: "character(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "token_hash",
                table: "refresh_tokens",
                type: "character(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "token_hash",
                table: "refresh_tokens");

            migrationBuilder.AlterColumn<string>(
                name: "token_hash",
                table: "user_invitations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character(64)",
                oldFixedLength: true,
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "refresh_tokens",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

                migrationBuilder.CreateIndex(
                    name: "ix_refresh_tokens_token",
                    table: "refresh_tokens",
                    column: "token",
                    unique: true);
            }
}
