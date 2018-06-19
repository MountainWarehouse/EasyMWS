using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class RenameColumnsInReportRequestEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastRequested",
                table: "ReportRequestEntries",
                newName: "LastAmazonRequestDate");

            migrationBuilder.RenameColumn(
                name: "InvokeCallbackRetryDownload",
                table: "ReportRequestEntries",
                newName: "InvokeCallbackRetryCount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastAmazonRequestDate",
                table: "ReportRequestEntries",
                newName: "LastRequested");

            migrationBuilder.RenameColumn(
                name: "InvokeCallbackRetryCount",
                table: "ReportRequestEntries",
                newName: "InvokeCallbackRetryDownload");
        }
    }
}
