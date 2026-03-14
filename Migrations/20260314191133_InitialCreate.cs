using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TaskManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Users_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserTenant",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTenant", x => new { x.UserId, x.TenantId });
                    table.ForeignKey(
                        name: "FK_UserTenant_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTenant_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Enabled", "Title" },
                values: new object[,]
                {
                    { new Guid("2337e27f-58eb-4973-9b43-4b795dac1ad7"), true, "Tenant-B" },
                    { new Guid("4da16ab8-3f6b-4af6-9fba-82daa779aeb9"), true, "Tenant-C" },
                    { new Guid("4da30340-fda0-49b0-b564-f511c630d221"), true, "Tenant-A" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "TenantId", "Username" },
                values: new object[,]
                {
                    { new Guid("13bac98c-abd2-424b-bf2f-e1c26dee0e71"), "frank@tenant-c.example.com", new Guid("00000000-0000-0000-0000-000000000000"), "frank" },
                    { new Guid("1b33930d-4437-41ee-9b10-a864b40cec78"), "carol@tenant-b.example.com", new Guid("00000000-0000-0000-0000-000000000000"), "carol" },
                    { new Guid("420f5b2e-ef55-4733-8bd9-053a07b9ed9c"), "dan@tenant-b.example.com", new Guid("00000000-0000-0000-0000-000000000000"), "dan" },
                    { new Guid("657ca4fa-fb2d-4180-80db-1403c6b8579e"), "bob@tenant-a.example.com", new Guid("00000000-0000-0000-0000-000000000000"), "bob" },
                    { new Guid("bef81bfc-2cbb-4321-bd4a-cecb244dadcb"), "alice@tenant-a.example.com", new Guid("00000000-0000-0000-0000-000000000000"), "alice" },
                    { new Guid("e8f46251-0c7a-47c2-99b3-88c9496d8e9d"), "grace@no-tenant.example.com", new Guid("00000000-0000-0000-0000-000000000000"), "grace" },
                    { new Guid("eaa46a1d-7ace-4ba8-9b2e-a4e4f8d654d1"), "eve@tenant-c.example.com", new Guid("00000000-0000-0000-0000-000000000000"), "eve" }
                });

            migrationBuilder.InsertData(
                table: "Tasks",
                columns: new[] { "Id", "AssignedUserId", "DueDate", "Status", "TenantId", "Title" },
                values: new object[,]
                {
                    { new Guid("c534787f-dfb8-4269-8941-791efcb8c4e4"), new Guid("bef81bfc-2cbb-4321-bd4a-cecb244dadcb"), null, 0, new Guid("00000000-0000-0000-0000-000000000000"), "Build Web API" },
                    { new Guid("d907410e-5860-4cc4-8800-2230895c001f"), new Guid("1b33930d-4437-41ee-9b10-a864b40cec78"), null, 0, new Guid("00000000-0000-0000-0000-000000000000"), "Learn ASP.NET Core" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedUserId",
                table: "Tasks",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TenantId",
                table: "Tasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                table: "Users",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTenant_TenantId",
                table: "UserTenant",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "UserTenant");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
