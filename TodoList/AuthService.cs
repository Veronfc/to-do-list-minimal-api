using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public class AuthService
{
  private readonly IConfiguration _configuration;

  public AuthService(IConfiguration configuration)
  {
    _configuration = configuration;
  }

  public string GenerateJwtToken(string username)
  {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DotNetEnv.Env.GetString("JWT_KEY")));

    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(JwtRegisteredClaimNames.Sub, username),
      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var token = new JwtSecurityToken
    (
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.Now.AddHours(12),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}