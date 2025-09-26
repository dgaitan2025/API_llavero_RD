using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ProyDesaWeb2025.Models;

namespace ProyDesaWeb2025.Security;

public sealed class JwtTokenService
{
    private readonly IConfiguration _cfg;
    public JwtTokenService(IConfiguration cfg) => _cfg = cfg;

    public string CreateToken(UsuarioCredencial u)
    {
        var issuer   = _cfg["Jwt:Issuer"]   ?? "ProyDesaWeb2025";
        var audience = _cfg["Jwt:Audience"] ?? "ProyDesaWeb2025.Frontend";
        var key      = _cfg["Jwt:Key"]      ?? throw new InvalidOperationException("Configura Jwt:Key en appsettings.json");
        var expMin   = int.TryParse(_cfg["Jwt:ExpMinutes"], out var m) ? m : 120;

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, u.UsuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, u.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.UniqueName, u.Nickname ?? string.Empty),
            new("rolId", u.RolId.ToString()),
            new("estaActivo", u.EstaActivo ? "1" : "0"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, u.RolId.ToString())
        };

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(expMin),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}