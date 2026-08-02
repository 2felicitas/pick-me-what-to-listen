using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickMeWhatToListen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMetadataSyncStatusDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Artists SET MetadataSyncStatus = 'None' WHERE MetadataSyncStatus = '' OR MetadataSyncStatus IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "MetadataSyncStatus",
                table: "Artists",
                type: "TEXT",
                nullable: false,
                defaultValue: "None",
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MetadataSyncStatus",
                table: "Artists",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "None");
        }
    }
}
