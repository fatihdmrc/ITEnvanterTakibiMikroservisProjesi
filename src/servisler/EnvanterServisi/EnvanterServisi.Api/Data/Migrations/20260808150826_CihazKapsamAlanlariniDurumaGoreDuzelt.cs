using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvanterServisi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CihazKapsamAlanlariniDurumaGoreDuzelt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH hesap AS (
                    SELECT
                        "Id",
                        (
                            "Durum" IN ('Kayip', 'Calindi', 'KullanimDisi')
                            OR ("Durum" = 'HurdaIskarta' AND COALESCE("EldenCikarmaTipi", 'Yok') <> 'Yok')
                        ) AS "EnvanterDisindaMi",
                        CASE
                            WHEN "Durum" IN ('Kayip', 'Calindi') THEN 'Yok'
                            WHEN "Durum" = 'KullanimDisi' AND COALESCE("EldenCikarmaTipi", 'Yok') = 'Yok' THEN 'Diger'
                            WHEN NOT (
                                "Durum" IN ('Kayip', 'Calindi', 'KullanimDisi')
                                OR ("Durum" = 'HurdaIskarta' AND COALESCE("EldenCikarmaTipi", 'Yok') <> 'Yok')
                            ) THEN 'Yok'
                            ELSE COALESCE("EldenCikarmaTipi", 'Yok')
                        END AS "YeniEldenCikarmaTipi"
                    FROM envanter."Cihazlar"
                )
                UPDATE envanter."Cihazlar" AS cihaz
                SET
                    "AktifMi" = NOT hesap."EnvanterDisindaMi",
                    "ToplamVarligaDahilMi" = NOT hesap."EnvanterDisindaMi",
                    "EnvanterdenCikisTarihi" = CASE
                        WHEN hesap."EnvanterDisindaMi" THEN COALESCE(cihaz."EnvanterdenCikisTarihi", CURRENT_DATE)
                        ELSE NULL
                    END,
                    "EldenCikarmaTipi" = hesap."YeniEldenCikarmaTipi",
                    "EldenCikarmaAciklamasi" = CASE
                        WHEN NOT hesap."EnvanterDisindaMi" OR cihaz."Durum" IN ('Kayip', 'Calindi') THEN NULL
                        ELSE cihaz."EldenCikarmaAciklamasi"
                    END,
                    "SatilanKisiVeyaKurum" = CASE
                        WHEN hesap."EnvanterDisindaMi" AND hesap."YeniEldenCikarmaTipi" = 'Satildi' THEN cihaz."SatilanKisiVeyaKurum"
                        ELSE NULL
                    END,
                    "GuncellenmeTarihi" = NOW()
                FROM hesap
                WHERE cihaz."Id" = hesap."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
