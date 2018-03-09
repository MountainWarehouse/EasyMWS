using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class AddedAmazonReportIDColumnsToReportRequestCallback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneratedReportId",
                table: "ReportRequestCallbacks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestReportId",
                table: "ReportRequestCallbacks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequestCallbacks_RequestReportId_GeneratedReportId",
                table: "ReportRequestCallbacks",
                columns: new[] { "RequestReportId", "GeneratedReportId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReportRequestCallbacks_RequestReportId_GeneratedReportId",
                table: "ReportRequestCallbacks");

            migrationBuilder.DropColumn(
                name: "GeneratedReportId",
                table: "ReportRequestCallbacks");

            migrationBuilder.DropColumn(
                name: "RequestReportId",
                table: "ReportRequestCallbacks");
        }
    }
}
