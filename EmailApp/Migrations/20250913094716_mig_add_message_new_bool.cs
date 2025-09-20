using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmailApp.Migrations
{
    /// <inheritdoc />
    public partial class mig_add_message_new_bool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFlagged",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsForwarded",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImportant",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReplied",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsFlagged",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsForwarded",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsImportant",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsReplied",
                table: "Messages");
        }
    }
}
