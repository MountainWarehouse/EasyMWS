using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class RenamedFeedTableColumnToHasErrors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsStatusDone",
                table: "FeedSubmissionCallbacks",
                newName: "HasErrors");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HasErrors",
                table: "FeedSubmissionCallbacks",
                newName: "IsStatusDone");
        }
    }
}
