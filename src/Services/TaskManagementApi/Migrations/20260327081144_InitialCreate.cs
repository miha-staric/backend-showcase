using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskManagementApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Tenants", x => x.Id);
                }
            );

            _ = migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Email = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Users", x => x.Id);
                }
            );

            _ = migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PrimaryAssigneeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Tasks", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Tasks_Users_PrimaryAssigneeId",
                        column: x => x.PrimaryAssigneeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull
                    );
                }
            );

            _ = migrationBuilder.CreateTable(
                name: "UserTenant",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    UserRole = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_UserTenant", x => new { x.UserId, x.TenantId });
                    _ = table.ForeignKey(
                        name: "FK_UserTenant_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            _ = migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UpdatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_Comments", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_Comments_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    _ = table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            _ = migrationBuilder.CreateTable(
                name: "UserTask",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    UserTenantUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserTenantTenantId = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_UserTask", x => new { x.UserId, x.TaskItemId });
                    _ = table.ForeignKey(
                        name: "FK_UserTask_Tasks_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    _ = table.ForeignKey(
                        name: "FK_UserTask_UserTenant_UserTenantUserId_UserTenantTenantId",
                        columns: x => new { x.UserTenantUserId, x.UserTenantTenantId },
                        principalTable: "UserTenant",
                        principalColumns: new[] { "UserId", "TenantId" }
                    );
                    _ = table.ForeignKey(
                        name: "FK_UserTask_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_Comments_TaskId",
                table: "Comments",
                column: "TaskId"
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_Comments_TaskId_CreatedAt",
                table: "Comments",
                columns: new[] { "TaskId", "CreatedAt" }
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId"
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_Tasks_PrimaryAssigneeId",
                table: "Tasks",
                column: "PrimaryAssigneeId"
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_Tasks_TenantId",
                table: "Tasks",
                column: "TenantId"
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                table: "Users",
                column: "Id",
                unique: true
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_UserTask_TaskItemId",
                table: "UserTask",
                column: "TaskItemId"
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_UserTask_TenantId",
                table: "UserTask",
                column: "TenantId"
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_UserTask_UserTenantUserId_UserTenantTenantId",
                table: "UserTask",
                columns: new[] { "UserTenantUserId", "UserTenantTenantId" }
            );

            _ = migrationBuilder.CreateIndex(
                name: "IX_UserTenant_TenantId_Username",
                table: "UserTenant",
                columns: new[] { "TenantId", "Username" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            _ = migrationBuilder.DropTable(name: "Comments");

            _ = migrationBuilder.DropTable(name: "Tenants");

            _ = migrationBuilder.DropTable(name: "UserTask");

            _ = migrationBuilder.DropTable(name: "Tasks");

            _ = migrationBuilder.DropTable(name: "UserTenant");

            _ = migrationBuilder.DropTable(name: "Users");
        }
    }
}
