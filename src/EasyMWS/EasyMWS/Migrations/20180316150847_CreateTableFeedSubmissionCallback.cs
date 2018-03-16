using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class CreateTableFeedSubmissionCallback : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedSubmissionCallbacks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AmazonRegion = table.Column<int>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    DataTypeName = table.Column<string>(nullable: true),
                    FeedSubmissionId = table.Column<string>(nullable: true),
                    MethodName = table.Column<string>(nullable: true),
                    ReportRequestData = table.Column<string>(nullable: true),
                    TypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedSubmissionCallbacks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedSubmissionCallbacks_FeedSubmissionId",
                table: "FeedSubmissionCallbacks",
                column: "FeedSubmissionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedSubmissionCallbacks");
        }
    }
}
