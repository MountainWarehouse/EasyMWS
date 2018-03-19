using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class RenamedColumnsInFeedSubmissionCallback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResultReceived",
                table: "FeedSubmissionCallbacks",
                newName: "IsStatusDone");

            migrationBuilder.RenameColumn(
                name: "ResultIsSuccess",
                table: "FeedSubmissionCallbacks",
                newName: "IsProcessingComplete");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsStatusDone",
                table: "FeedSubmissionCallbacks",
                newName: "ResultReceived");

            migrationBuilder.RenameColumn(
                name: "IsProcessingComplete",
                table: "FeedSubmissionCallbacks",
                newName: "ResultIsSuccess");
        }
    }
}
