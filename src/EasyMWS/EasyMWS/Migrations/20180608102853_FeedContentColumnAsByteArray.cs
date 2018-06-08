using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class FeedContentColumnAsByteArray : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FeedContent",
                table: "FeedSubmissionDetails",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeedContent",
                table: "FeedSubmissionDetails");
        }
    }
}
