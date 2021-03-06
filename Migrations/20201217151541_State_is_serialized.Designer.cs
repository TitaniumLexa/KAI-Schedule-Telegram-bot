﻿// <auto-generated />
using System;
using KAI_Schedule.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KAI_Schedule.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20201217151541_State_is_serialized")]
    partial class State_is_serialized
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("KAI_Schedule.Data.ChatContext", b =>
                {
                    b.Property<long>("ChatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Group")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("StateSerialized")
                        .HasColumnType("BLOB");

                    b.HasKey("ChatId");

                    b.ToTable("ChatContexts");
                });

            modelBuilder.Entity("KAI_Schedule.Data.ScheduleDbEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Group")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastUpdate")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("ScheduleBinary")
                        .HasColumnType("BLOB");

                    b.HasKey("Id");

                    b.ToTable("Schedules");
                });
#pragma warning restore 612, 618
        }
    }
}
