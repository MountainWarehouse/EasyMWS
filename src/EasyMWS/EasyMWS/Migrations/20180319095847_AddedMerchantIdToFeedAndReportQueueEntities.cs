using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class AddedMerchantIdToFeedAndReportQueueEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestRetryCount",
                table: "FeedSubmissionCallbacks",
                newName: "SubmissionRetryCount");

            migrationBuilder.AddColumn<string>(
                name: "MerchantId",
                table: "ReportRequestCallbacks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MerchantId",
                table: "FeedSubmissionCallbacks",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "ReportRequestCallbacks");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "FeedSubmissionCallbacks");

            migrationBuilder.RenameColumn(
                name: "SubmissionRetryCount",
                table: "FeedSubmissionCallbacks",
                newName: "RequestRetryCount");
        }
    }
}
