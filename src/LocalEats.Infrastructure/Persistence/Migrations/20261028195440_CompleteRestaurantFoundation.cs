using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LocalEats.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CompleteRestaurantFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Restaurants",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpen",
                table: "Restaurants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Area",
                table: "Restaurants",
                column: "Area");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_Cuisine",
                table: "Restaurants",
                column: "Cuisine");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Restaurants_Name_NotBlank",
                table: "Restaurants",
                sql: "length(btrim(\"Name\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Restaurants_Slug_NotBlank",
                table: "Restaurants",
                sql: "length(btrim(\"Slug\")) > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Area",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_Cuisine",
                table: "Restaurants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Restaurants_Name_NotBlank",
                table: "Restaurants");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Restaurants_Slug_NotBlank",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "IsOpen",
                table: "Restaurants");
        }
    }
}
