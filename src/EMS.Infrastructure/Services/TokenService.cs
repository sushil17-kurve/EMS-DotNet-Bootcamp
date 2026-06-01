using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EMS.Application.DTOs.Auth;
using EMS.Application.Interfaces.Services;
using EMS.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EMS.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;

    public TokenService(IOptions<JwtSettings> jwt)
    {
        _jwt = jwt.Value;
    }

    public string GenerateAccessToken(User user)
    {
        // Claims = data embedded inside the token
        // These are readable by the API without hitting the DB
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Name,           user.FullName),
            new(ClaimTypes.Role,           user.Role.ToString()),
            // Custom claim for employee-specific checks
            new("userId",                  user.Id.ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        // Cryptographically secure random bytes → base64 string
        // Much more secure than a simple GUID
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public int GetUserIdFromExpiredToken(string token)
    {
        // Validate token signature even if expired
        // This prevents using a tampered token to get a new one
        var tokenValidationParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(_jwt.Secret)),
            ValidateIssuer = true,
            ValidIssuer = _jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwt.Audience,
            ValidateLifetime = false  // ← Allow expired tokens here
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(
                             token, tokenValidationParams, out var validatedToken);

        // Ensure it's actually a JWT with correct algorithm
        if (validatedToken is not JwtSecurityToken jwtToken ||
            !jwtToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            throw new SecurityTokenException("Invalid token claims");

        return userId;
    }
}