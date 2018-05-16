using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class MoveFeedContentToSeparateTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedSubmissionDetails",
                columns: table => new
                {
                    FeedSubmissionEntryId = table.Column<int>(nullable: false),
                    FeedContent = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedSubmissionDetails", x => x.FeedSubmissionEntryId);
                    table.ForeignKey(
                        name: "FK_FeedSubmissionDetails_FeedSubmissionEntries_FeedSubmissionEntryId",
                        column: x => x.FeedSubmissionEntryId,
                        principalTable: "FeedSubmissionEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedSubmissionDetails");
        }
    }
}
