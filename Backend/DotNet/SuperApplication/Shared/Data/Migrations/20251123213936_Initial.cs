using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperApplication.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<int>(type: "integer", nullable: false),
                    Co2 = table.Column<int>(type: "integer", nullable: true),
                    Pm25 = table.Column<int>(type: "integer", nullable: true),
                    Humidity = table.Column<int>(type: "integer", nullable: true),
                    MotionDetected = table.Column<bool>(type: "boolean", nullable: true),
                    Energy = table.Column<double>(type: "double precision", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorReadings");
        }
    }
}
