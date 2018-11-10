using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class AddLastProcessingStatusToEntryTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastAmazonReportProcessingStatus",
                table: "ReportRequestEntries",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastAmazonFeedProcessingStatus",
                table: "FeedSubmissionEntries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastAmazonReportProcessingStatus",
                table: "ReportRequestEntries");

            migrationBuilder.DropColumn(
                name: "LastAmazonFeedProcessingStatus",
                table: "FeedSubmissionEntries");
        }
    }
}
