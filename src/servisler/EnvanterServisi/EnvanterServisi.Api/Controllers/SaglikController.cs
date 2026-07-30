using Microsoft.AspNetCore.Mvc;

namespace EnvanterServisi.Api.Controllers;

[ApiController]
[Route("saglik")]
public sealed class SaglikController : ControllerBase
{
    [HttpGet]
    public ActionResult<object> Getir()
    {
        return Ok(new
        {
            servisAdi = "EnvanterServisi",
            durum = "Calisiyor",
            utcZamani = DateTimeOffset.UtcNow
        });
    }
}
