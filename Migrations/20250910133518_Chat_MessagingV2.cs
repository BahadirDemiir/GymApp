using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymAppFresh.Migrations
{
    /// <inheritdoc />
    public partial class Chat_MessagingV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GymId",
                table: "ChatThreads",
                newName: "GymLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatThreads_GymId_MemberId_GymStaffId",
                table: "ChatThreads",
                newName: "IX_ChatThreads_GymLocationId_MemberId_GymStaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GymLocationId",
                table: "ChatThreads",
                newName: "GymId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatThreads_GymLocationId_MemberId_GymStaffId",
                table: "ChatThreads",
                newName: "IX_ChatThreads_GymId_MemberId_GymStaffId");
        }
    }
}
