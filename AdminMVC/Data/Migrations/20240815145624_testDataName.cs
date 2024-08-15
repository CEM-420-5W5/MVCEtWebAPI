using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdminMVC.Data.Migrations
{
    /// <inheritdoc />
    public partial class testDataName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "TestDatas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "TestDatas");
        }
    }
}
