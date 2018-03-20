using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class AddedRetryLogicRelatedFieldsToFeedsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSubmitted",
                table: "FeedSubmissionCallbacks",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "RequestRetryCount",
                table: "FeedSubmissionCallbacks",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSubmitted",
                table: "FeedSubmissionCallbacks");

            migrationBuilder.DropColumn(
                name: "RequestRetryCount",
                table: "FeedSubmissionCallbacks");
        }
    }
}
