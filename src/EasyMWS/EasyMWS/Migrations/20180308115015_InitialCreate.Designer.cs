﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using MountainWarehouse.EasyMWS;
using MountainWarehouse.EasyMWS.Data;
using MountainWarehouse.EasyMWS.Helpers;
using System;

namespace MountainWarehouse.EasyMWS.Migrations
{
    [DbContext(typeof(EasyMwsContext))]
    [Migration("20180308115015_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MountainWarehouse.EasyMWS.Data.ReportRequestCallback", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AmazonRegion");

                    b.Property<int>("ContentUpdateFrequency");

                    b.Property<string>("Data");

                    b.Property<string>("DataTypeName");

                    b.Property<DateTime?>("LastRequested");

                    b.Property<string>("MethodName");

                    b.Property<string>("ReportType");

                    b.Property<string>("TypeName");

                    b.HasKey("Id");

                    b.ToTable("ReportRequestCallbacks");
                });
#pragma warning restore 612, 618
        }
    }
}