using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickMeWhatToListen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArtistMetadataEnrichment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MetadataSyncError",
                table: "Artists",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetadataSyncStatus",
                table: "Artists",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "MetadataSyncedAtUtc",
                table: "Artists",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MusicBrainzArtistMbid",
                table: "Artists",
                type: "TEXT",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WikidataUrl",
                table: "Artists",
                type: "TEXT",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetadataTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetadataTerms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReleaseGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MusicBrainzReleaseGroupMbid = table.Column<string>(type: "TEXT", maxLength: 36, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PrimaryType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    FirstReleaseDate = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    CoverReleaseMbid = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    CoverArtUrl = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    CoverArtStatus = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReleaseGroups_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistMetadataTerms",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MetadataTermId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistMetadataTerms", x => new { x.ArtistId, x.MetadataTermId, x.Kind });
                    table.ForeignKey(
                        name: "FK_ArtistMetadataTerms_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistMetadataTerms_MetadataTerms_MetadataTermId",
                        column: x => x.MetadataTermId,
                        principalTable: "MetadataTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artists_MusicBrainzArtistMbid",
                table: "Artists",
                column: "MusicBrainzArtistMbid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtistMetadataTerms_MetadataTermId",
                table: "ArtistMetadataTerms",
                column: "MetadataTermId");

            migrationBuilder.CreateIndex(
                name: "IX_MetadataTerms_Name",
                table: "MetadataTerms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseGroups_ArtistId_FirstReleaseDate",
                table: "ReleaseGroups",
                columns: new[] { "ArtistId", "FirstReleaseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseGroups_ArtistId_MusicBrainzReleaseGroupMbid",
                table: "ReleaseGroups",
                columns: new[] { "ArtistId", "MusicBrainzReleaseGroupMbid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistMetadataTerms");

            migrationBuilder.DropTable(
                name: "ReleaseGroups");

            migrationBuilder.DropTable(
                name: "MetadataTerms");

            migrationBuilder.DropIndex(
                name: "IX_Artists_MusicBrainzArtistMbid",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "MetadataSyncError",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "MetadataSyncStatus",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "MetadataSyncedAtUtc",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "MusicBrainzArtistMbid",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "WikidataUrl",
                table: "Artists");
        }
    }
}
