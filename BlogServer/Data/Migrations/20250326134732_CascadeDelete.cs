using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_AspNetUsers_ContributorId",
                table: "Articles");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_AspNetUsers_ContributorId",
                table: "Articles",
                column: "ContributorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_AspNetUsers_ContributorId",
                table: "Articles");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_AspNetUsers_ContributorId",
                table: "Articles",
                column: "ContributorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
