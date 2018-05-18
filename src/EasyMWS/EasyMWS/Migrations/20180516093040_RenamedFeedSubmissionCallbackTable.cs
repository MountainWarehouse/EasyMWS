using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class RenamedFeedSubmissionCallbackTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedSubmissionCallbacks");

            migrationBuilder.CreateTable(
                name: "FeedSubmissionEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AmazonRegion = table.Column<int>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    DataTypeName = table.Column<string>(nullable: true),
                    FeedSubmissionData = table.Column<string>(nullable: true),
                    FeedSubmissionId = table.Column<string>(nullable: true),
                    FeedType = table.Column<string>(nullable: true),
                    HasErrors = table.Column<bool>(nullable: false),
                    IsProcessingComplete = table.Column<bool>(nullable: false),
                    LastSubmitted = table.Column<DateTime>(nullable: false),
                    MerchantId = table.Column<string>(nullable: true),
                    MethodName = table.Column<string>(nullable: true),
                    SubmissionErrorData = table.Column<string>(nullable: true),
                    SubmissionRetryCount = table.Column<int>(nullable: false),
                    TypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedSubmissionEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedSubmissionEntries_FeedSubmissionId",
                table: "FeedSubmissionEntries",
                column: "FeedSubmissionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedSubmissionEntries");

            migrationBuilder.CreateTable(
                name: "FeedSubmissionCallbacks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AmazonRegion = table.Column<int>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    DataTypeName = table.Column<string>(nullable: true),
                    FeedSubmissionData = table.Column<string>(nullable: true),
                    FeedSubmissionId = table.Column<string>(nullable: true),
                    FeedType = table.Column<string>(nullable: true),
                    HasErrors = table.Column<bool>(nullable: false),
                    IsProcessingComplete = table.Column<bool>(nullable: false),
                    LastSubmitted = table.Column<DateTime>(nullable: false),
                    MerchantId = table.Column<string>(nullable: true),
                    MethodName = table.Column<string>(nullable: true),
                    SubmissionErrorData = table.Column<string>(nullable: true),
                    SubmissionRetryCount = table.Column<int>(nullable: false),
                    TypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedSubmissionCallbacks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedSubmissionCallbacks_FeedSubmissionId",
                table: "FeedSubmissionCallbacks",
                column: "FeedSubmissionId");
        }
    }
}
