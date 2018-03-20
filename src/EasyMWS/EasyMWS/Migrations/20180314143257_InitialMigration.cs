using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportRequestCallbacks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AmazonRegion = table.Column<int>(nullable: false),
                    ContentUpdateFrequency = table.Column<int>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    DataTypeName = table.Column<string>(nullable: true),
                    GeneratedReportId = table.Column<string>(nullable: true),
                    LastRequested = table.Column<DateTime>(nullable: true),
                    MethodName = table.Column<string>(nullable: true),
                    ReportRequestData = table.Column<string>(nullable: true),
                    RequestReportId = table.Column<string>(nullable: true),
                    RequestRetryCount = table.Column<int>(nullable: false),
                    TypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequestCallbacks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportRequestCallbacks_RequestReportId_GeneratedReportId",
                table: "ReportRequestCallbacks",
                columns: new[] { "RequestReportId", "GeneratedReportId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportRequestCallbacks");
        }
    }
}
