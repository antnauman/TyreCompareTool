using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TyreCompare.DAL.Interfaces;
using TyreCompare.Models;

namespace TyreCompare.API.Controllers;

[Authorize]
[ApiController]
[ApiVersion("0.5"), ApiVersion("1.0"), ApiVersion("1.5")]
[Route("api/[controller]"), Route("api/v{version:apiVersion}/[controller]")]
public class SummaryController : ControllerBase
{
    private ITyreCompareService DataAccessLayer;

    public SummaryController(ITyreCompareService dataAccessLayer)
    {
        DataAccessLayer = dataAccessLayer;
    }

    [HttpGet]
    public IActionResult GetAllSumamry(bool includeObsolete = false)
    {
        var summary = DataAccessLayer.GetCompleteSummary(includeObsolete);
        return Ok(summary);
    }

    [HttpGet("{brand}")]
    public IActionResult GetBrandSumamry([FromRoute] string brand, bool includeObsolete = false)
    {
        if (string.IsNullOrWhiteSpace(brand))
        { throw new ArgumentException("Parameters are invalid."); }

        var summary = DataAccessLayer.GetSummaryByBrand(brand, includeObsolete);
        return Ok(summary);
    }

    [HttpPost("ByPage")]
    public IActionResult GetBrandSumamry([FromBody] PaginationQuery paginationQuery)
    {
        if (paginationQuery == null)
        { throw new ArgumentException("Parameter is invalid."); }

        var summary = DataAccessLayer.GetSummaryByPage(paginationQuery);
        return Ok(summary);
    }
}
