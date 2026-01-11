using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusEats.Api.Common.Interfaces;
using CampusEats.Api.Data.Entities;
using Microsoft.IdentityModel.Tokens;

namespace CampusEats.Api.Common.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration config)
    {
        var tokenKey =
            Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? throw new InvalidOperationException(
                "JWT secret key is missing. Set the JWT_SECRET_KEY environment variable."
            );
        
        if (tokenKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT secret key must be at least 32 characters long."
            );
        }
        
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
    }

    public string CreateToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.GivenName, user.UserName!),
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "CampusEats.Api",
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
