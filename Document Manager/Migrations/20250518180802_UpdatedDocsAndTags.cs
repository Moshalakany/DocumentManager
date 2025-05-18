using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Document_Manager.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDocsAndTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupPermissionsUsers_Users_UserId1",
                table: "GroupPermissionsUsers");

            migrationBuilder.DropIndex(
                name: "IX_GroupPermissionsUsers_UserId1",
                table: "GroupPermissionsUsers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "GroupPermissionsUsers");

            migrationBuilder.AddColumn<bool>(
                name: "CanAnnotate",
                table: "UserAccessibilityListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDelete",
                table: "UserAccessibilityListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanDownload",
                table: "UserAccessibilityListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanEdit",
                table: "UserAccessibilityListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanShare",
                table: "UserAccessibilityListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanView",
                table: "UserAccessibilityListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "GroupPermissionsUsers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissionsUsers_UserId",
                table: "GroupPermissionsUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPermissionsUsers_Users_UserId",
                table: "GroupPermissionsUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupPermissionsUsers_Users_UserId",
                table: "GroupPermissionsUsers");

            migrationBuilder.DropIndex(
                name: "IX_GroupPermissionsUsers_UserId",
                table: "GroupPermissionsUsers");

            migrationBuilder.DropColumn(
                name: "CanAnnotate",
                table: "UserAccessibilityListItems");

            migrationBuilder.DropColumn(
                name: "CanDelete",
                table: "UserAccessibilityListItems");

            migrationBuilder.DropColumn(
                name: "CanDownload",
                table: "UserAccessibilityListItems");

            migrationBuilder.DropColumn(
                name: "CanEdit",
                table: "UserAccessibilityListItems");

            migrationBuilder.DropColumn(
                name: "CanShare",
                table: "UserAccessibilityListItems");

            migrationBuilder.DropColumn(
                name: "CanView",
                table: "UserAccessibilityListItems");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "GroupPermissionsUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "GroupPermissionsUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupPermissionsUsers_UserId1",
                table: "GroupPermissionsUsers",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupPermissionsUsers_Users_UserId1",
                table: "GroupPermissionsUsers",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
