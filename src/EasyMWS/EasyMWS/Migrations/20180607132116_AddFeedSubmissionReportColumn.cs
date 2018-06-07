using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class AddFeedSubmissionReportColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FeedSubmissionReport",
                table: "FeedSubmissionDetails",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedSubmissionReport",
                table: "FeedSubmissionDetails");
        }
    }
}
