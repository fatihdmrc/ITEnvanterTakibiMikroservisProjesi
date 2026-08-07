using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvanterServisi.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssetTagBosCihazlariDoldur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH mevcut AS (
                    SELECT COALESCE(MAX(substring("AssetTag" from '^BT-(\d+)$')::integer), 0) AS "SonSira"
                    FROM envanter."Cihazlar"
                    WHERE "AssetTag" ~ '^BT-\d+$'
                ),
                bos AS (
                    SELECT
                        "Id",
                        row_number() OVER (ORDER BY "OlusturulmaTarihi", "Id") AS "Sira"
                    FROM envanter."Cihazlar"
                    WHERE "AssetTag" IS NULL OR btrim("AssetTag") = ''
                )
                UPDATE envanter."Cihazlar" AS cihaz
                SET "AssetTag" = 'BT-' || lpad((mevcut."SonSira" + bos."Sira")::text, 6, '0')
                FROM bos
                CROSS JOIN mevcut
                WHERE cihaz."Id" = bos."Id";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
