using EMS.Domain.Entities;

namespace EMS.Application.Interfaces.Services;

// Separated from AuthService — Single Responsibility Principle
// Token generation is its own concern
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    int GetUserIdFromExpiredToken(string token); // For refresh flow
}