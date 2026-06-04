using DigitalMenu.Infrastructure.Services;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalMenu.Infrastructure.Migrations
{
    /// <summary>
    /// Mevcut satırlardaki localhost QR tabanını production /r yoluna çevirir (tenant ve table query korunur).
    /// Özel <c>QrMenu__BaseUrl</c> için uygulama açılışında DbInitializer.RefreshQrCodeUrlsAsync çalışır.
    /// </summary>
    public partial class ReplaceLocalhostQrCodeUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var productionBase = QrMenuUrlBuilder.ProductionDefaultBaseUrl.TrimEnd('/').Replace("'", "''", StringComparison.Ordinal);

            migrationBuilder.Sql(
                $"""
                UPDATE tables
                SET qr_code_url = regexp_replace(
                    qr_code_url,
                    '^https?://[^/]+/r',
                    '{productionBase}',
                    'i')
                WHERE qr_code_url ~* 'localhost|127\.0\.0\.1';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
