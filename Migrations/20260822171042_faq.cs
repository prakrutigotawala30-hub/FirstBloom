using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FirstBloom.Migrations
{
    /// <inheritdoc />
    public partial class faq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FAQs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Question = table.Column<string>(
                        maxLength: 250,
                        nullable: false),

                    Answer = table.Column<string>(
                        nullable: false),

                    Icon = table.Column<string>(
                        maxLength: 50,
                        nullable: false),

                    Color = table.Column<string>(
                        maxLength: 20,
                        nullable: false),

                    IsActive = table.Column<bool>(
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FAQs");
        }
    }
}