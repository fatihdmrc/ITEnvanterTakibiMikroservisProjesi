using Microsoft.AspNetCore.Mvc;

namespace DenetimKaydiServisi.Api.Controllers;

[ApiController]
[Route("saglik")]
public sealed class SaglikController : ControllerBase
{
    [HttpGet]
    public ActionResult<object> Getir()
    {
        return Ok(new
        {
            servisAdi = "DenetimKaydiServisi",
            durum = "Calisiyor",
            utcZamani = DateTimeOffset.UtcNow
        });
    }
}
