using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedSubmissionEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsLocked = table.Column<bool>(nullable: false),
                    FeedSubmissionRetryCount = table.Column<int>(nullable: false),
                    FeedProcessingRetryCount = table.Column<int>(nullable: false),
                    ReportDownloadRetryCount = table.Column<int>(nullable: false),
                    InvokeCallbackRetryCount = table.Column<int>(nullable: false),
                    LastSubmitted = table.Column<DateTime>(nullable: false),
                    LastAmazonFeedProcessingStatus = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    TargetHandlerId = table.Column<string>(nullable: true),
                    TargetHandlerArgs = table.Column<string>(nullable: true),
                    InstanceId = table.Column<string>(nullable: true),
                    AmazonRegion = table.Column<int>(nullable: false),
                    FeedType = table.Column<string>(nullable: true),
                    MerchantId = table.Column<string>(nullable: true),
                    FeedSubmissionData = table.Column<string>(nullable: true),
                    FeedSubmissionId = table.Column<string>(nullable: true),
                    IsProcessingComplete = table.Column<bool>(nullable: false),
                    HasErrors = table.Column<bool>(nullable: false),
                    SubmissionErrorData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedSubmissionEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportRequestEntries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsLocked = table.Column<bool>(nullable: false),
                    ReportRequestRetryCount = table.Column<int>(nullable: false),
                    ReportDownloadRetryCount = table.Column<int>(nullable: false),
                    InvokeCallbackRetryCount = table.Column<int>(nullable: false),
                    ReportProcessRetryCount = table.Column<int>(nullable: false),
                    LastAmazonRequestDate = table.Column<DateTime>(nullable: false),
                    LastAmazonReportProcessingStatus = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    TargetHandlerId = table.Column<string>(nullable: true),
                    TargetHandlerArgs = table.Column<string>(nullable: true),
                    InstanceId = table.Column<string>(nullable: true),
                    AmazonRegion = table.Column<int>(nullable: false),
                    ReportType = table.Column<string>(nullable: true),
                    MerchantId = table.Column<string>(nullable: true),
                    ContentUpdateFrequency = table.Column<int>(nullable: false),
                    ReportRequestData = table.Column<string>(nullable: true),
                    RequestReportId = table.Column<string>(nullable: true),
                    GeneratedReportId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequestEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedSubmissionDetails",
                columns: table => new
                {
                    FeedContent = table.Column<byte[]>(nullable: true),
                    FeedSubmissionReport = table.Column<byte[]>(nullable: true),
                    FeedSubmissionEntryId = table.Column<int>(nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ReportRequestDetails",
                columns: table => new
                {
                    ReportContent = table.Column<byte[]>(nullable: true),
                    ReportRequestEntryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequestDetails", x => x.ReportRequestEntryId);
                    table.ForeignKey(
                        name: "FK_ReportRequestDetails_ReportRequestEntries_ReportRequestEntryId",
                        column: x => x.ReportRequestEntryId,
                        principalTable: "ReportRequestEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedSubmissionEntries_FeedSubmissionId",
                table: "FeedSubmissionEntries",
                column: "FeedSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequestEntries_RequestReportId_GeneratedReportId",
                table: "ReportRequestEntries",
                columns: new[] { "RequestReportId", "GeneratedReportId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedSubmissionDetails");

            migrationBuilder.DropTable(
                name: "ReportRequestDetails");

            migrationBuilder.DropTable(
                name: "FeedSubmissionEntries");

            migrationBuilder.DropTable(
                name: "ReportRequestEntries");
        }
    }
}
