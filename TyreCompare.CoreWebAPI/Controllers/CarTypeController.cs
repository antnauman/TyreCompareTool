using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TyreCompare.DAL.Interfaces;
using TyreCompare.Models;

namespace TyreCompare.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("0.5"), ApiVersion("1.0"), ApiVersion("1.5")]
[Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class CarTypeController : ControllerBase
{
    private ITyreCompareService DataAccessLayer;

    public CarTypeController(ITyreCompareService dataAccessLayer)
    {
        DataAccessLayer = dataAccessLayer;
    }

    [HttpGet]
    public IActionResult GetAllCarTypes()
    {
        var patternSets = DataAccessLayer.GetAllCarTypes();
        return Ok(patternSets);
    }

    [HttpGet("{brand}")]
    public IActionResult GetCarTypesByBrand([FromRoute] string brand)
    {
        if (string.IsNullOrWhiteSpace(brand))
        { throw new ArgumentException("Parameters are invalid."); }

        var carTypesList = DataAccessLayer.GetCarTypesByBrand(brand);
        return Ok(carTypesList);
    }

}
