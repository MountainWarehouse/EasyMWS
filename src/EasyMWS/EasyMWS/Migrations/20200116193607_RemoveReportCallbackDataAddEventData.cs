using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class RemoveReportCallbackDataAddEventData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "ReportRequestEntries");

            migrationBuilder.DropColumn(
                name: "DataTypeName",
                table: "ReportRequestEntries");

            migrationBuilder.RenameColumn(
                name: "TypeName",
                table: "ReportRequestEntries",
                newName: "TargetHandlerId");

            migrationBuilder.RenameColumn(
                name: "MethodName",
                table: "ReportRequestEntries",
                newName: "TargetHandlerArgs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetHandlerId",
                table: "ReportRequestEntries",
                newName: "TypeName");

            migrationBuilder.RenameColumn(
                name: "TargetHandlerArgs",
                table: "ReportRequestEntries",
                newName: "MethodName");

            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "ReportRequestEntries",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataTypeName",
                table: "ReportRequestEntries",
                nullable: true);
        }
    }
}
