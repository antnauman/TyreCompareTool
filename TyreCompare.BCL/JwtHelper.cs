using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TyreCompare.BCL;

public static class JwtHelper
{
    public static byte[]? GetJwtKey()
    {
        return Encoding.ASCII.GetBytes("-your-secret-key-is-here-your-secret-key-is-here");
    }

    public static string GenerateJwtTokenForUser(string username, string userRole)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, userRole)
        };
        var claimsIdentity = new ClaimsIdentity(claims);

        var key = JwtHelper.GetJwtKey();
        var securityKey = new SymmetricSecurityKey(key);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Expires = DateTime.Now.AddHours(24),
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}
