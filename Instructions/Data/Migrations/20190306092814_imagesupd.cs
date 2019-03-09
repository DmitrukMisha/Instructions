using Microsoft.EntityFrameworkCore.Migrations;

namespace Instructions.Data.Migrations
{
    public partial class imagesupd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_Records_RecordID1",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_RecordID1",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "RecordID1",
                table: "Images");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecordID1",
                table: "Images",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Images_RecordID1",
                table: "Images",
                column: "RecordID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_Records_RecordID1",
                table: "Images",
                column: "RecordID1",
                principalTable: "Records",
                principalColumn: "RecordID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
