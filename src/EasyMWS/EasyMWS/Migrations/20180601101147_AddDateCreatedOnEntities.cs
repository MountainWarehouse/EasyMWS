using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class AddDateCreatedOnEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "ReportRequestEntries",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "FeedSubmissionEntries",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "ReportRequestEntries");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "FeedSubmissionEntries");
        }
    }
}
