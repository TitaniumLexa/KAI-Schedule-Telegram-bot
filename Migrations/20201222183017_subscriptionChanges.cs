using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KAI_Schedule.Migrations
{
    public partial class subscriptionChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNotified",
                table: "Subscriptions");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotificationTime",
                table: "Subscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "NotificationInterval",
                table: "Subscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastNotificationTime",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "NotificationInterval",
                table: "Subscriptions");

            migrationBuilder.AddColumn<bool>(
                name: "IsNotified",
                table: "Subscriptions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
