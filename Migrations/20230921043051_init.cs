using System;
using Microsoft.EntityFrameworkCore.Migrations;
using webapi_80.src.Tenant.SchemaTenant.SchemaContext;

#nullable disable

namespace webapi_80.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        private readonly IDbContextSchema _schema;
        private string _Schema;

        public init(IDbContextSchema schema)
        {
            _schema = schema ?? throw new ArgumentNullException(nameof(schema));
            _Schema = _schema.Schema;
        }
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: _Schema);

            migrationBuilder.CreateTable(
                name: "Tenants",
                schema: _Schema,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subdomain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isSchemaCreated = table.Column<bool>(type: "bit", nullable: false),
                    LastMigration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdminId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: _Schema,
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tenants",
                schema: _Schema);

            migrationBuilder.DropTable(
                name: "Users",
                schema: _Schema);
        }
    }
}
