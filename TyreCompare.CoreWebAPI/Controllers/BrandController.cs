using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TyreCompare.DAL.Interfaces;

namespace TyreCompare.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("0.5"), ApiVersion("1.0"), ApiVersion("1.5")]
[Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class BrandController : ControllerBase
{
    private ITyreCompareService DataAccessLayer;

    public BrandController(ITyreCompareService dataAccessLayer)
    {
        DataAccessLayer = dataAccessLayer;
    }

    [HttpGet]
    public IActionResult GetAllBrandNames()
    {
        var patternSets = DataAccessLayer.GetAllBrandNames();
        return Ok(patternSets);
    }
}
