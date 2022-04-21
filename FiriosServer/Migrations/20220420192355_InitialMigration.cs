using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FiriosServer.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncidentEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Mpd = table.Column<string>(type: "TEXT", nullable: false),
                    Region = table.Column<string>(type: "TEXT", nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    SubType = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    ObjectName = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalInformation = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Titules = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", nullable: false),
                    SecondName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "BLOB", nullable: false),
                    PasswordIteration = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBrowserDatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Session = table.Column<string>(type: "TEXT", nullable: false),
                    Expire = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Endpoint = table.Column<string>(type: "TEXT", nullable: false),
                    P256dh = table.Column<string>(type: "TEXT", nullable: false),
                    Auth = table.Column<string>(type: "TEXT", nullable: false),
                    UserEntityId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBrowserDatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBrowserDatas_UserEntity_UserEntityId",
                        column: x => x.UserEntityId,
                        principalTable: "UserEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserIncidentEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncidentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIncidentEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIncidentEntity_IncidentEntity_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "IncidentEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserIncidentEntity_UserEntity_UserId",
                        column: x => x.UserId,
                        principalTable: "UserEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBrowserDatas_UserEntityId",
                table: "UserBrowserDatas",
                column: "UserEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEntity_Email",
                table: "UserEntity",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserIncidentEntity_IncidentId",
                table: "UserIncidentEntity",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIncidentEntity_UserId",
                table: "UserIncidentEntity",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBrowserDatas");

            migrationBuilder.DropTable(
                name: "UserIncidentEntity");

            migrationBuilder.DropTable(
                name: "IncidentEntity");

            migrationBuilder.DropTable(
                name: "UserEntity");
        }
    }
}
