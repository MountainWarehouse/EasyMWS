using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class AddRedundantReportAndFeedTypeOnCallbackEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportType",
                table: "ReportRequestCallbacks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedType",
                table: "FeedSubmissionCallbacks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportType",
                table: "ReportRequestCallbacks");

            migrationBuilder.DropColumn(
                name: "FeedType",
                table: "FeedSubmissionCallbacks");
        }
    }
}
