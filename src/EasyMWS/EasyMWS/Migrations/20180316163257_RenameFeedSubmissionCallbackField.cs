using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class RenameFeedSubmissionCallbackField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReportRequestData",
                table: "FeedSubmissionCallbacks",
                newName: "FeedSubmissionData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FeedSubmissionData",
                table: "FeedSubmissionCallbacks",
                newName: "ReportRequestData");
        }
    }
}
