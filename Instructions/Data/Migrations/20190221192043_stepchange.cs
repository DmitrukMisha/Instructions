using Microsoft.EntityFrameworkCore.Migrations;

namespace Instructions.Data.Migrations
{
    public partial class stepchange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordID",
                table: "Steps");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Steps",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordID1",
                table: "Steps",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Steps_RecordID1",
                table: "Steps",
                column: "RecordID1");

            migrationBuilder.AddForeignKey(
                name: "FK_Steps_Records_RecordID1",
                table: "Steps",
                column: "RecordID1",
                principalTable: "Records",
                principalColumn: "RecordID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Steps_Records_RecordID1",
                table: "Steps");

            migrationBuilder.DropIndex(
                name: "IX_Steps_RecordID1",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Steps");

            migrationBuilder.DropColumn(
                name: "RecordID1",
                table: "Steps");

            migrationBuilder.AddColumn<int>(
                name: "RecordID",
                table: "Steps",
                nullable: false,
                defaultValue: 0);
        }
    }
}
