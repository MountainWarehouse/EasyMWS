using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class AddExtraRetryColums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestRetryCount",
                table: "ReportRequestEntries",
                newName: "ReportRequestRetryCount");

            migrationBuilder.RenameColumn(
                name: "SubmissionRetryCount",
                table: "FeedSubmissionEntries",
                newName: "ReportDownloadRetryCount");

            migrationBuilder.AddColumn<int>(
                name: "InvokeCallbackRetryDownload",
                table: "ReportRequestEntries",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReportDownloadRetryCount",
                table: "ReportRequestEntries",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FeedProcessingRetryCount",
                table: "FeedSubmissionEntries",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FeedSubmissionRetryCount",
                table: "FeedSubmissionEntries",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InvokeCallbackRetryCount",
                table: "FeedSubmissionEntries",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvokeCallbackRetryDownload",
                table: "ReportRequestEntries");

            migrationBuilder.DropColumn(
                name: "ReportDownloadRetryCount",
                table: "ReportRequestEntries");

            migrationBuilder.DropColumn(
                name: "FeedProcessingRetryCount",
                table: "FeedSubmissionEntries");

            migrationBuilder.DropColumn(
                name: "FeedSubmissionRetryCount",
                table: "FeedSubmissionEntries");

            migrationBuilder.DropColumn(
                name: "InvokeCallbackRetryCount",
                table: "FeedSubmissionEntries");

            migrationBuilder.RenameColumn(
                name: "ReportRequestRetryCount",
                table: "ReportRequestEntries",
                newName: "RequestRetryCount");

            migrationBuilder.RenameColumn(
                name: "ReportDownloadRetryCount",
                table: "FeedSubmissionEntries",
                newName: "SubmissionRetryCount");
        }
    }
}
