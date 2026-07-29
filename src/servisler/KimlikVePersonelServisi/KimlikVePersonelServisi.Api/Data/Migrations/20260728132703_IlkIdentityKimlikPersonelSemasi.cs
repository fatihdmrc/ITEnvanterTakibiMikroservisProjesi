using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace KimlikVePersonelServisi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class IlkIdentityKimlikPersonelSemasi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kimlik_personel");

            migrationBuilder.CreateTable(
                name: "Roller",
                schema: "kimlik_personel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roller", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolClaimleri",
                schema: "kimlik_personel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolClaimleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolClaimleri_Roller_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "kimlik_personel",
                        principalTable: "Roller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departmanlar",
                schema: "kimlik_personel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ad = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SorumluPersonelId = table.Column<Guid>(type: "uuid", nullable: true),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departmanlar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personeller",
                schema: "kimlik_personel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Soyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DepartmanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Unvan = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DepartmanSorumlusuMu = table.Column<bool>(type: "boolean", nullable: false),
                    Durum = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IseGirisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    IstenAyrilisTarihi = table.Column<DateOnly>(type: "date", nullable: true),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personeller", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Personeller_Departmanlar_DepartmanId",
                        column: x => x.DepartmanId,
                        principalSchema: "kimlik_personel",
                        principalTable: "Departmanlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                schema: "kimlik_personel",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonelId = table.Column<Guid>(type: "uuid", nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kullanicilar_Personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalSchema: "kimlik_personel",
                        principalTable: "Personeller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciClaimleri",
                schema: "kimlik_personel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciClaimleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KullaniciClaimleri_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalSchema: "kimlik_personel",
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciLoginleri",
                schema: "kimlik_personel",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciLoginleri", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_KullaniciLoginleri_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalSchema: "kimlik_personel",
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciRolleri",
                schema: "kimlik_personel",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciRolleri", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_KullaniciRolleri_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalSchema: "kimlik_personel",
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KullaniciRolleri_Roller_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "kimlik_personel",
                        principalTable: "Roller",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciTokenlari",
                schema: "kimlik_personel",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciTokenlari", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_KullaniciTokenlari_Kullanicilar_UserId",
                        column: x => x.UserId,
                        principalSchema: "kimlik_personel",
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departmanlar_Ad",
                schema: "kimlik_personel",
                table: "Departmanlar",
                column: "Ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departmanlar_SorumluPersonelId",
                schema: "kimlik_personel",
                table: "Departmanlar",
                column: "SorumluPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciClaimleri_UserId",
                schema: "kimlik_personel",
                table: "KullaniciClaimleri",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "kimlik_personel",
                table: "Kullanicilar",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_PersonelId",
                schema: "kimlik_personel",
                table: "Kullanicilar",
                column: "PersonelId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "kimlik_personel",
                table: "Kullanicilar",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciLoginleri_UserId",
                schema: "kimlik_personel",
                table: "KullaniciLoginleri",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciRolleri_RoleId",
                schema: "kimlik_personel",
                table: "KullaniciRolleri",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_DepartmanId",
                schema: "kimlik_personel",
                table: "Personeller",
                column: "DepartmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_Email",
                schema: "kimlik_personel",
                table: "Personeller",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolClaimleri_RoleId",
                schema: "kimlik_personel",
                table: "RolClaimleri",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "kimlik_personel",
                table: "Roller",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departmanlar_Personeller_SorumluPersonelId",
                schema: "kimlik_personel",
                table: "Departmanlar",
                column: "SorumluPersonelId",
                principalSchema: "kimlik_personel",
                principalTable: "Personeller",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departmanlar_Personeller_SorumluPersonelId",
                schema: "kimlik_personel",
                table: "Departmanlar");

            migrationBuilder.DropTable(
                name: "KullaniciClaimleri",
                schema: "kimlik_personel");

            migrationBuilder.DropTable(
                name: "KullaniciLoginleri",
                schema: "kimlik_personel");

            migrationBuilder.DropTable(
                name: "KullaniciRolleri",
                schema: "kimlik_personel");

            migrationBuilder.DropTable(
                name: "KullaniciTokenlari",
                schema: "kimlik_personel");

            migrationBuilder.DropTable(
                name: "RolClaimleri",
                schema: "kimlik_personel");

            migrationBuilder.DropTable(
                name: "Kullanicilar",
                schema: "kimlik_personel");

            migrationBuilder.DropTable(
                name: "Roller",
                schema: "kimlik_personel");

            migrationBuilder.DropTable(
                name: "Personeller",
                schema: "kimlik_personel");

            migrationBuilder.DropTable(
                name: "Departmanlar",
                schema: "kimlik_personel");
        }
    }
}
