using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZimmetServisi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class IlkZimmetSemasi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "zimmet");

            migrationBuilder.CreateTable(
                name: "Zimmetler",
                schema: "zimmet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CihazId = table.Column<Guid>(type: "uuid", nullable: false),
                    CihazAd = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    CihazAssetTag = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CihazSeriNumarasi = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PersonelId = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonelAdSoyad = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    PersonelEmail = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ZimmetTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    ZimmetleyenKullaniciId = table.Column<Guid>(type: "uuid", nullable: false),
                    IadeTarihi = table.Column<DateOnly>(type: "date", nullable: true),
                    IadeAlanKullaniciId = table.Column<Guid>(type: "uuid", nullable: true),
                    IadeKontrolDurumu = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    IadeKontroluYapanKullaniciId = table.Column<Guid>(type: "uuid", nullable: true),
                    IadeNotu = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Durum = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zimmetler", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_CihazId",
                schema: "zimmet",
                table: "Zimmetler",
                column: "CihazId",
                unique: true,
                filter: "\"Durum\" IN ('Aktif', 'IadeSurecinde')");

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_Durum",
                schema: "zimmet",
                table: "Zimmetler",
                column: "Durum");

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_PersonelId",
                schema: "zimmet",
                table: "Zimmetler",
                column: "PersonelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Zimmetler",
                schema: "zimmet");
        }
    }
}
