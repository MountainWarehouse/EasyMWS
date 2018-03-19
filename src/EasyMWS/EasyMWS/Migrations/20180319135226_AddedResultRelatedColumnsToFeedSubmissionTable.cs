using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class AddedResultRelatedColumnsToFeedSubmissionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ResultIsSuccess",
                table: "FeedSubmissionCallbacks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ResultReceived",
                table: "FeedSubmissionCallbacks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionErrorData",
                table: "FeedSubmissionCallbacks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultIsSuccess",
                table: "FeedSubmissionCallbacks");

            migrationBuilder.DropColumn(
                name: "ResultReceived",
                table: "FeedSubmissionCallbacks");

            migrationBuilder.DropColumn(
                name: "SubmissionErrorData",
                table: "FeedSubmissionCallbacks");
        }
    }
}
