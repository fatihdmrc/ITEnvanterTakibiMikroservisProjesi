using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvanterServisi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class IlkEnvanterSemasi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "envanter");

            migrationBuilder.CreateTable(
                name: "Kategoriler",
                schema: "envanter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ad = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    UstKategoriId = table.Column<Guid>(type: "uuid", nullable: true),
                    VarlikTuru = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    KritikStokSeviyesi = table.Column<int>(type: "integer", nullable: true),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategoriler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Kategoriler_Kategoriler_UstKategoriId",
                        column: x => x.UstKategoriId,
                        principalSchema: "envanter",
                        principalTable: "Kategoriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KritikStokKurallari",
                schema: "envanter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LokasyonId = table.Column<Guid>(type: "uuid", nullable: false),
                    KategoriId = table.Column<Guid>(type: "uuid", nullable: false),
                    CihazModeli = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    KritikStokSeviyesi = table.Column<int>(type: "integer", nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KritikStokKurallari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lokasyonlar",
                schema: "envanter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ad = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    UstLokasyonId = table.Column<Guid>(type: "uuid", nullable: true),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lokasyonlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lokasyonlar_Lokasyonlar_UstLokasyonId",
                        column: x => x.UstLokasyonId,
                        principalSchema: "envanter",
                        principalTable: "Lokasyonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StokHareketleri",
                schema: "envanter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CihazId = table.Column<Guid>(type: "uuid", nullable: true),
                    SarfMalzemeId = table.Column<Guid>(type: "uuid", nullable: true),
                    HareketTipi = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Neden = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Miktar = table.Column<int>(type: "integer", nullable: true),
                    Aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OlusturanKullaniciId = table.Column<Guid>(type: "uuid", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokHareketleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cihazlar",
                schema: "envanter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeriNumarasi = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    AssetTag = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Marka = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    KategoriId = table.Column<Guid>(type: "uuid", nullable: false),
                    LokasyonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Durum = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    EnvantereGirisTarihi = table.Column<DateOnly>(type: "date", nullable: false),
                    EnvanterdenCikisTarihi = table.Column<DateOnly>(type: "date", nullable: true),
                    EldenCikarmaTipi = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EldenCikarmaAciklamasi = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SatilanKisiVeyaKurum = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false),
                    ToplamVarligaDahilMi = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cihazlar", x => x.Id);
                    table.CheckConstraint("CK_Cihazlar_SeriNumarasi_Veya_AssetTag", "\"SeriNumarasi\" IS NOT NULL OR \"AssetTag\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_Cihazlar_Kategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalSchema: "envanter",
                        principalTable: "Kategoriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cihazlar_Lokasyonlar_LokasyonId",
                        column: x => x.LokasyonId,
                        principalSchema: "envanter",
                        principalTable: "Lokasyonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SarfMalzemeler",
                schema: "envanter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    KategoriId = table.Column<Guid>(type: "uuid", nullable: false),
                    LokasyonId = table.Column<Guid>(type: "uuid", nullable: false),
                    EldekiMiktar = table.Column<int>(type: "integer", nullable: false),
                    KritikStokSeviyesi = table.Column<int>(type: "integer", nullable: false),
                    Birim = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AktifMi = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellenmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SarfMalzemeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SarfMalzemeler_Kategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalSchema: "envanter",
                        principalTable: "Kategoriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SarfMalzemeler_Lokasyonlar_LokasyonId",
                        column: x => x.LokasyonId,
                        principalSchema: "envanter",
                        principalTable: "Lokasyonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cihazlar_AssetTag",
                schema: "envanter",
                table: "Cihazlar",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cihazlar_KategoriId",
                schema: "envanter",
                table: "Cihazlar",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_Cihazlar_LokasyonId",
                schema: "envanter",
                table: "Cihazlar",
                column: "LokasyonId");

            migrationBuilder.CreateIndex(
                name: "IX_Cihazlar_SeriNumarasi",
                schema: "envanter",
                table: "Cihazlar",
                column: "SeriNumarasi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_Ad_UstKategoriId",
                schema: "envanter",
                table: "Kategoriler",
                columns: new[] { "Ad", "UstKategoriId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kategoriler_UstKategoriId",
                schema: "envanter",
                table: "Kategoriler",
                column: "UstKategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_KritikStokKurallari_LokasyonId_KategoriId_CihazModeli",
                schema: "envanter",
                table: "KritikStokKurallari",
                columns: new[] { "LokasyonId", "KategoriId", "CihazModeli" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lokasyonlar_Ad_UstLokasyonId",
                schema: "envanter",
                table: "Lokasyonlar",
                columns: new[] { "Ad", "UstLokasyonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lokasyonlar_UstLokasyonId",
                schema: "envanter",
                table: "Lokasyonlar",
                column: "UstLokasyonId");

            migrationBuilder.CreateIndex(
                name: "IX_SarfMalzemeler_Ad_KategoriId_LokasyonId",
                schema: "envanter",
                table: "SarfMalzemeler",
                columns: new[] { "Ad", "KategoriId", "LokasyonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SarfMalzemeler_KategoriId",
                schema: "envanter",
                table: "SarfMalzemeler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_SarfMalzemeler_LokasyonId",
                schema: "envanter",
                table: "SarfMalzemeler",
                column: "LokasyonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cihazlar",
                schema: "envanter");

            migrationBuilder.DropTable(
                name: "KritikStokKurallari",
                schema: "envanter");

            migrationBuilder.DropTable(
                name: "SarfMalzemeler",
                schema: "envanter");

            migrationBuilder.DropTable(
                name: "StokHareketleri",
                schema: "envanter");

            migrationBuilder.DropTable(
                name: "Kategoriler",
                schema: "envanter");

            migrationBuilder.DropTable(
                name: "Lokasyonlar",
                schema: "envanter");
        }
    }
}
