using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class RefactorReportStorage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmazonReports");

            migrationBuilder.CreateTable(
                name: "ReportRequestDetails",
                columns: table => new
                {
                    ReportContent = table.Column<byte[]>(nullable: true),
                    ReportRequestEntryId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequestDetails", x => x.ReportRequestEntryId);
                    table.ForeignKey(
                        name: "FK_ReportRequestDetails_ReportRequestEntries_ReportRequestEntryId",
                        column: x => x.ReportRequestEntryId,
                        principalTable: "ReportRequestEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportRequestDetails");

            migrationBuilder.CreateTable(
                name: "AmazonReports",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    DateCreated = table.Column<DateTime>(nullable: false),
                    DownloadRequestId = table.Column<string>(nullable: true),
                    DownloadTimestamp = table.Column<string>(nullable: true),
                    ReportType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmazonReports", x => x.Id);
                });
        }
    }
}
