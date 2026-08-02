using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvanterServisi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CihazDurumuEskiDegerleriniGuncelle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE envanter."Cihazlar"
                SET "Durum" = CASE "Durum"
                    WHEN 'DepodaHazir' THEN 'Kullanilabilir'
                    WHEN 'Arizali' THEN 'Bakimda'
                    WHEN 'HurdaIskartaDepoda' THEN 'HurdaIskarta'
                    WHEN 'EldenCikarildi' THEN 'KullanimDisi'
                    ELSE "Durum"
                END
                WHERE "Durum" IN ('DepodaHazir', 'Arizali', 'HurdaIskartaDepoda', 'EldenCikarildi');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE envanter."Cihazlar"
                SET "Durum" = CASE "Durum"
                    WHEN 'Kullanilabilir' THEN 'DepodaHazir'
                    WHEN 'Bakimda' THEN 'Arizali'
                    WHEN 'HurdaIskarta' THEN 'HurdaIskartaDepoda'
                    WHEN 'KullanimDisi' THEN 'EldenCikarildi'
                    ELSE "Durum"
                END
                WHERE "Durum" IN ('Kullanilabilir', 'Bakimda', 'HurdaIskarta', 'KullanimDisi');
                """);
        }
    }
}
