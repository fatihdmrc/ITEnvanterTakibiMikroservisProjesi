using Microsoft.AspNetCore.Mvc;

namespace ZimmetServisi.Api.Controllers;

[ApiController]
[Route("saglik")]
public sealed class SaglikController : ControllerBase
{
    [HttpGet]
    public ActionResult<object> Getir()
    {
        return Ok(new
        {
            servis = "ZimmetServisi",
            durum = "Calisiyor",
            zaman = DateTimeOffset.UtcNow
        });
    }
}
