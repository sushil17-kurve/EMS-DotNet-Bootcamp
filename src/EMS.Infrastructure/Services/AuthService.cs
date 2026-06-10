using EMS.Application.DTOs.Auth;
using EMS.Application.DTOs.Common;
using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Services;
using EMS.Domain.Entities;
using EMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwt;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork uow,
        ITokenService tokenService,
        IOptions<JwtSettings> jwt,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _tokenService = tokenService;
        _jwt = jwt.Value;
        _logger = logger;
    }

    public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

        var user = await _uow.Users.GetByEmailAsync(dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
            return ApiResponseDto<AuthResponseDto>.Fail("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Inactive user login attempt: {Email}", dto.Email);
            return ApiResponseDto<AuthResponseDto>.Fail(
                "Your account has been deactivated. Contact HR.");
        }

        _logger.LogInformation("Successful login for user: {UserId} {Email}",
            user.Id, user.Email);

        return await GenerateAuthResponseAsync(user, "Login successful.");
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", dto.Email);

        var exists = await _uow.Users.ExistsAsync(
            u => u.Email.ToLower() == dto.Email.ToLower());

        if (exists)
            return ApiResponseDto<AuthResponseDto>.Fail(
                "An account with this email already exists.");

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            Role = UserRole.Employee,
            IsActive = true
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("New user registered: {UserId} {Email}", user.Id, user.Email);
        return await GenerateAuthResponseAsync(user, "Registration successful.");
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
    {
        int userId;
        try
        {
            userId = _tokenService.GetUserIdFromExpiredToken(dto.AccessToken);
        }
        catch
        {
            _logger.LogWarning("Invalid access token used for refresh");
            return ApiResponseDto<AuthResponseDto>.Fail("Invalid access token.");
        }

        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponseDto<AuthResponseDto>.Fail("User not found.");

        if (user.RefreshToken != dto.RefreshToken)
        {
            _logger.LogWarning("Refresh token mismatch for user: {UserId}", userId);
            return ApiResponseDto<AuthResponseDto>.Fail("Invalid refresh token.");
        }

        if (user.RefreshTokenExpiry <= DateTime.UtcNow)
            return ApiResponseDto<AuthResponseDto>.Fail(
                "Refresh token expired. Please login again.");

        return await GenerateAuthResponseAsync(user, "Token refreshed.");
    }

    public async Task<ApiResponseDto<bool>> LogoutAsync(int userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponseDto<bool>.Fail("User not found.");

        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        _logger.LogInformation("User logged out: {UserId}", userId);
        return ApiResponseDto<bool>.Ok(true, "Logged out successfully.");
    }

    private async Task<ApiResponseDto<AuthResponseDto>> GenerateAuthResponseAsync(
        User user, string message)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
        user.UpdatedAt = DateTime.UtcNow;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return ApiResponseDto<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            User = new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                ProfilePhotoPath = user.ProfilePhotoPath
            }
        }, message);
    }
}