using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiMonitor.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Apis",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                                         .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    Url = table.Column<string>(maxLength: 2000, nullable: false),
                    StatusCode = table.Column<int>(nullable: true),
                    ResponseTimeMs = table.Column<long>(nullable: true),
                    IsRunning = table.Column<bool>(nullable: false),
                    YamlSource = table.Column<string>(maxLength: 500, nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Apis", x => x.Id));

            migrationBuilder.CreateTable(
                name: "ApiEndpoints",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                                         .Annotation("SqlServer:Identity", "1, 1"),
                    ApiId = table.Column<int>(nullable: false),
                    Method = table.Column<string>(maxLength: 10, nullable: false),
                    Path = table.Column<string>(maxLength: 500, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    SchemaFile = table.Column<string>(maxLength: 500, nullable: true),
                    StatusCode = table.Column<int>(nullable: true),
                    ResponseTimeMs = table.Column<long>(nullable: true),
                    IsRunning = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiEndpoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiEndpoints_Apis_ApiId",
                        column: x => x.ApiId,
                        principalTable: "Apis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiEndpoints_ApiId",
                table: "ApiEndpoints",
                column: "ApiId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ApiEndpoints");
            migrationBuilder.DropTable(name: "Apis");
        }
    }
}
