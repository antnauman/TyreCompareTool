using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TyreCompare.BCL;
using TyreCompare.DAL.Interfaces;
using TyreCompare.Models.CustomModels;
using ILog = TyreCompare.Log.ILog;

namespace TyreCompare.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private ITyreCompareService DataAccessLayer;
    private ILog Logger;

    public UserController(ITyreCompareService dataAccessLayer, ILog logger)
    {
        DataAccessLayer = dataAccessLayer;
        Logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        string message = "This is a GET request. Please send POST for login.";
        return Ok(message);
    }

    [HttpPost]
    [AllowAnonymous]
    public IActionResult LoginUser([FromBody] UserCredentials userCredentials)
    {
        if (string.IsNullOrWhiteSpace(userCredentials?.Username) || string.IsNullOrWhiteSpace(userCredentials?.Password))
        { throw new ArgumentException("Parameters are invalid."); }

        var currentUser = DataAccessLayer.ValidateUser(userCredentials?.Username, userCredentials?.Password);
        if (currentUser == null)
        { return BadRequest("Username or Password is invalid."); }

        var token = JwtHelper.GenerateJwtTokenForUser(currentUser.Username, currentUser.UserRoleName);
        Logger.Log($"{userCredentials?.Username} | Logged in.");
        return Ok(token);
    }
}
