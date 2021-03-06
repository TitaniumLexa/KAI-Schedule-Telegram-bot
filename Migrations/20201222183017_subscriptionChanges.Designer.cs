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
    [Migration("20201222183017_subscriptionChanges")]
    partial class subscriptionChanges
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

            modelBuilder.Entity("KAI_Schedule.Data.Subscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<long?>("ChatContextChatId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastNotificationTime")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("NotificationInterval")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("NotificationTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ChatContextChatId");

                    b.ToTable("Subscriptions");
                });

            modelBuilder.Entity("KAI_Schedule.Data.Subscription", b =>
                {
                    b.HasOne("KAI_Schedule.Data.ChatContext", "ChatContext")
                        .WithMany("Subscriptions")
                        .HasForeignKey("ChatContextChatId");

                    b.Navigation("ChatContext");
                });

            modelBuilder.Entity("KAI_Schedule.Data.ChatContext", b =>
                {
                    b.Navigation("Subscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
