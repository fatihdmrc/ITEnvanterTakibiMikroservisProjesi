using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnvanterServisi.Api.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(EnvanterDbContext))]
    [Migration("20260814120000_KritikStokKurallariniSeriNumaraliKategoriyleSinirla")]
    public partial class KritikStokKurallariniSeriNumaraliKategoriyleSinirla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE envanter."KritikStokKurallari" AS kural
                SET
                    "AktifMi" = FALSE,
                    "GuncellenmeTarihi" = NOW()
                FROM envanter."Kategoriler" AS kategori
                WHERE
                    kural."KategoriId" = kategori."Id"
                    AND kategori."VarlikTuru" <> 'SeriNumarali'
                    AND kural."AktifMi" = TRUE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
