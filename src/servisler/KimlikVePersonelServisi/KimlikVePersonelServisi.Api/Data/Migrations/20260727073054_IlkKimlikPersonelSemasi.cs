using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KimlikVePersonelServisi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class IlkKimlikPersonelSemasi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kimlik_personel");

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
                    KullaniciAdi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SifreHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PersonelId = table.Column<Guid>(type: "uuid", nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "IX_Kullanicilar_KullaniciAdi",
                schema: "kimlik_personel",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_PersonelId",
                schema: "kimlik_personel",
                table: "Kullanicilar",
                column: "PersonelId",
                unique: true);

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
                name: "Kullanicilar",
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
