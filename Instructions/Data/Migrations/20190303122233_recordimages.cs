using Microsoft.EntityFrameworkCore.Migrations;

namespace Instructions.Data.Migrations
{
    public partial class recordimages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageLink",
                table: "Records",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageLink",
                table: "Records");
        }
    }
}
