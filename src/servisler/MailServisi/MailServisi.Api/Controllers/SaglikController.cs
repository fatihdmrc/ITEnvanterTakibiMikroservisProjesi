using Microsoft.AspNetCore.Mvc;

namespace MailServisi.Api.Controllers;

[ApiController]
[Route("saglik")]
public sealed class SaglikController : ControllerBase
{
    [HttpGet]
    public ActionResult<object> Getir()
    {
        return Ok(new
        {
            servisAdi = "MailServisi",
            durum = "Calisiyor",
            utcZamani = DateTimeOffset.UtcNow
        });
    }
}
