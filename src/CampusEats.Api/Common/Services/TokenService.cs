using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CampusEats.Api.Common.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        _config = config;
        var tokenKey = _config["Token:Key"] ?? throw new InvalidOperationException(
            "Token:Key configuration is missing. Please add it to appsettings.json or appsettings.Development.json");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
    }
    
    public string CreateToken(ApplicationUser user, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Role, role)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
            Issuer = _config["Token:Issuer"]
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        return tokenHandler.WriteToken(token);
    }
}