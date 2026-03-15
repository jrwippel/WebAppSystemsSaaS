using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppSystems.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subdomain = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Document = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrialEndsAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxUsers = table.Column<int>(type: "int", nullable: false),
                    MaxClients = table.Column<int>(type: "int", nullable: false),
                    MaxStorageMB = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActivityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityTypes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Document = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ImageMimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Solicitante = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClienteInterno = table.Column<bool>(type: "bit", nullable: false),
                    ClienteInativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Client_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Department_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parametros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    LogoData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    LogoMimeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parametros_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Mensalista",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    ValorMensalBruto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComissaoParceiro = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComissaoSocio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensalista", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensalista_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Mensalista_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attorney",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Perfil = table.Column<int>(type: "int", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisterDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Login = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseBorder = table.Column<bool>(type: "bit", nullable: false),
                    UseCronometroAlwaysOnTop = table.Column<bool>(type: "bit", nullable: false),
                    Inativo = table.Column<bool>(type: "bit", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attorney", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attorney_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Attorney_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PercentualArea",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Percentual = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PercentualArea", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PercentualArea_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PercentualArea_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PercentualArea_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentAnalysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedByAttorneyId = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Complexity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EstimatedHours = table.Column<int>(type: "int", nullable: true),
                    MainTopics = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalBasis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CauseValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Deadlines = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecommendedAttorneys = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnalysisStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AnalysisDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedToAttorneyId = table.Column<int>(type: "int", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClientId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentAnalysis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentAnalysis_Attorney_AssignedToAttorneyId",
                        column: x => x.AssignedToAttorneyId,
                        principalTable: "Attorney",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentAnalysis_Attorney_UploadedByAttorneyId",
                        column: x => x.UploadedByAttorneyId,
                        principalTable: "Attorney",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentAnalysis_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentAnalysis_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcessRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttorneyId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    HoraInicial = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFinal = table.Column<TimeSpan>(type: "time", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Solicitante = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActivityTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessRecord_ActivityTypes_ActivityTypeId",
                        column: x => x.ActivityTypeId,
                        principalTable: "ActivityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRecord_Attorney_AttorneyId",
                        column: x => x.AttorneyId,
                        principalTable: "Attorney",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRecord_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRecord_Department_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcessRecord_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ValorCliente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    AttorneyId = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<double>(type: "float", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValorCliente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValorCliente_Attorney_AttorneyId",
                        column: x => x.AttorneyId,
                        principalTable: "Attorney",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValorCliente_Client_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Client",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValorCliente_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_TenantId",
                table: "ActivityTypes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Attorney_DepartmentId",
                table: "Attorney",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Attorney_TenantId",
                table: "Attorney",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Client_TenantId",
                table: "Client",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_TenantId",
                table: "Department",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnalysis_AssignedToAttorneyId",
                table: "DocumentAnalysis",
                column: "AssignedToAttorneyId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnalysis_ClientId",
                table: "DocumentAnalysis",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnalysis_TenantId",
                table: "DocumentAnalysis",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentAnalysis_UploadedByAttorneyId",
                table: "DocumentAnalysis",
                column: "UploadedByAttorneyId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensalista_ClientId",
                table: "Mensalista",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensalista_TenantId",
                table: "Mensalista",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Parametros_TenantId",
                table: "Parametros",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PercentualArea_ClientId",
                table: "PercentualArea",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PercentualArea_DepartmentId",
                table: "PercentualArea",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PercentualArea_TenantId",
                table: "PercentualArea",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRecord_ActivityTypeId",
                table: "ProcessRecord",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRecord_AttorneyId",
                table: "ProcessRecord",
                column: "AttorneyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRecord_ClientId",
                table: "ProcessRecord",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRecord_DepartmentId",
                table: "ProcessRecord",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRecord_TenantId",
                table: "ProcessRecord",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Subdomain",
                table: "Tenants",
                column: "Subdomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ValorCliente_AttorneyId",
                table: "ValorCliente",
                column: "AttorneyId");

            migrationBuilder.CreateIndex(
                name: "IX_ValorCliente_ClientId",
                table: "ValorCliente",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ValorCliente_TenantId",
                table: "ValorCliente",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentAnalysis");

            migrationBuilder.DropTable(
                name: "Mensalista");

            migrationBuilder.DropTable(
                name: "Parametros");

            migrationBuilder.DropTable(
                name: "PercentualArea");

            migrationBuilder.DropTable(
                name: "ProcessRecord");

            migrationBuilder.DropTable(
                name: "ValorCliente");

            migrationBuilder.DropTable(
                name: "ActivityTypes");

            migrationBuilder.DropTable(
                name: "Attorney");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
