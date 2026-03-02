using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServiceName.Infrastructure.Migrations;

public partial class M01_InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create SampleEntities table
        migrationBuilder.CreateTable(
            name: "SampleEntities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),

                name = table.Column<string>(
                    type: "nvarchar(200)",
                    maxLength: 200,
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SampleEntities", x => x.Id);
            });

        // Add index for fast lookup
        migrationBuilder.CreateIndex(
            name: "IX_SampleEntities_Name",
            table: "SampleEntities",
            column: "name");

    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Remove table (rollback)
        migrationBuilder.DropTable(
            name: "SampleEntities");
    }
}
