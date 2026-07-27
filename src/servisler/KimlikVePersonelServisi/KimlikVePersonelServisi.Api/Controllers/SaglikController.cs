using Microsoft.AspNetCore.Mvc;

namespace KimlikVePersonelServisi.Api.Controllers;

[ApiController]
[Route("saglik")]
public sealed class SaglikController : ControllerBase
{
    [HttpGet]
    public IActionResult Getir()
    {
        return Ok(new
        {
            servisAdi = "KimlikVePersonelServisi",
            durum = "Calisiyor",
            utcZamani = DateTimeOffset.UtcNow
        });
    }
}
