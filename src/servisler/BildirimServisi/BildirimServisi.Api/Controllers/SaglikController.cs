using Microsoft.AspNetCore.Mvc;

namespace BildirimServisi.Api.Controllers;

[ApiController]
[Route("saglik")]
public sealed class SaglikController : ControllerBase
{
    [HttpGet]
    public ActionResult<object> Getir()
    {
        return Ok(new
        {
            servisAdi = "BildirimServisi",
            durum = "Calisiyor",
            utcZamani = DateTimeOffset.UtcNow
        });
    }
}
