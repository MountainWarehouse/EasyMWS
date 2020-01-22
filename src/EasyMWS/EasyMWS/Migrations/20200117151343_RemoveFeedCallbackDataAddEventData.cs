using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class RemoveFeedCallbackDataAddEventData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "FeedSubmissionEntries");

            migrationBuilder.DropColumn(
                name: "DataTypeName",
                table: "FeedSubmissionEntries");

            migrationBuilder.RenameColumn(
                name: "TypeName",
                table: "FeedSubmissionEntries",
                newName: "TargetHandlerId");

            migrationBuilder.RenameColumn(
                name: "MethodName",
                table: "FeedSubmissionEntries",
                newName: "TargetHandlerArgs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetHandlerId",
                table: "FeedSubmissionEntries",
                newName: "TypeName");

            migrationBuilder.RenameColumn(
                name: "TargetHandlerArgs",
                table: "FeedSubmissionEntries",
                newName: "MethodName");

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "FeedSubmissionEntries",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataTypeName",
                table: "FeedSubmissionEntries",
                nullable: true);
        }
    }
}
