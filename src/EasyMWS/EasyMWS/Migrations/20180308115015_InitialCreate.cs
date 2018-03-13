using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MountainWarehouse.EasyMWS.Migrations
{
    public partial class InitialCreate : Migration
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
                    LastRequested = table.Column<DateTime>(nullable: true),
                    MethodName = table.Column<string>(nullable: true),
                    ReportType = table.Column<string>(nullable: true),
                    TypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRequestCallbacks", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportRequestCallbacks");
        }
    }
}
