using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KAI_Schedule.Migrations
{
    public partial class State_is_serialized : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StateTypeName",
                table: "ChatContexts");

            migrationBuilder.AddColumn<byte[]>(
                name: "StateSerialized",
                table: "ChatContexts",
                type: "BLOB",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StateSerialized",
                table: "ChatContexts");

            migrationBuilder.AddColumn<string>(
                name: "StateTypeName",
                table: "ChatContexts",
                type: "TEXT",
                nullable: true);
        }
    }
}
