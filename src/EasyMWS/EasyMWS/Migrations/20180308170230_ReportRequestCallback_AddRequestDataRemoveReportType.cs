using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class ReportRequestCallback_AddRequestDataRemoveReportType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReportType",
                table: "ReportRequestCallbacks",
                newName: "ReportRequestData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReportRequestData",
                table: "ReportRequestCallbacks",
                newName: "ReportType");
        }
    }
}
