using Microsoft.EntityFrameworkCore.Migrations;

namespace Instructions.Data.Migrations
{
    public partial class Comments3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Records_RecordID1",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_RecordID1",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "RecordID1",
                table: "Comments");

            migrationBuilder.AddColumn<int>(
                name: "RecordID",
                table: "Comments",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordID",
                table: "Comments");

            migrationBuilder.AddColumn<int>(
                name: "RecordID1",
                table: "Comments",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_RecordID1",
                table: "Comments",
                column: "RecordID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Records_RecordID1",
                table: "Comments",
                column: "RecordID1",
                principalTable: "Records",
                principalColumn: "RecordID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
