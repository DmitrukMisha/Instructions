using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Instructions.Data.Migrations
{
    public partial class SLILI : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Records",
                columns: table => new
                {
                    RecordID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    USerID = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    ThemeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Records", x => x.RecordID);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Themes = table.Column<string>(nullable: true),
                    ThemeID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.ThemeID);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Text = table.Column<string>(nullable: false),
                    StepName = table.Column<string>(nullable: false),
                    StepID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RecordID1 = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.StepID);
                    table.ForeignKey(
                        name: "FK_Steps_Records_RecordID1",
                        column: x => x.RecordID1,
                        principalTable: "Records",
                        principalColumn: "RecordID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RecordID = table.Column<int>(nullable: true),
                    TagName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagID);
                    table.ForeignKey(
                        name: "FK_Tags_Records_RecordID",
                        column: x => x.RecordID,
                        principalTable: "Records",
                        principalColumn: "RecordID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Steps_RecordID1",
                table: "Steps",
                column: "RecordID1");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_RecordID",
                table: "Tags",
                column: "RecordID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropTable(
                name: "Records");
        }
    }
}
