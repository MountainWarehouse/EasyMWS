using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class RemoveInstanceIdFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstanceId",
                table: "ReportRequestEntries");

            migrationBuilder.DropColumn(
                name: "InstanceId",
                table: "FeedSubmissionEntries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstanceId",
                table: "ReportRequestEntries",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstanceId",
                table: "FeedSubmissionEntries",
                nullable: true);
        }
    }
}
