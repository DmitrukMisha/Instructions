using Microsoft.EntityFrameworkCore.Migrations;

namespace Instructions.Data.Migrations
{
    public partial class comments2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Records_RecordID",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "RecordID",
                table: "Comments",
                newName: "RecordID1");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_RecordID",
                table: "Comments",
                newName: "IX_Comments_RecordID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Records_RecordID1",
                table: "Comments",
                column: "RecordID1",
                principalTable: "Records",
                principalColumn: "RecordID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Records_RecordID1",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "RecordID1",
                table: "Comments",
                newName: "RecordID");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_RecordID1",
                table: "Comments",
                newName: "IX_Comments_RecordID");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Records_RecordID",
                table: "Comments",
                column: "RecordID",
                principalTable: "Records",
                principalColumn: "RecordID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
