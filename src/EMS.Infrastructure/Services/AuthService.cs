using EMS.Application.DTOs.Auth;
using EMS.Application.DTOs.Common;
using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Services;
using EMS.Domain.Entities;
using EMS.Domain.Enums;
using Microsoft.Extensions.Options;

namespace EMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork   _uow;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings   _jwt;

    public AuthService(
        IUnitOfWork            uow,
        ITokenService          tokenService,
        IOptions<JwtSettings>  jwt)
    {
        _uow          = uow;
        _tokenService = tokenService;
        _jwt          = jwt.Value;
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        // Check email uniqueness
        var exists = await _uow.Users.ExistsAsync(
            u => u.Email.ToLower() == dto.Email.ToLower());

        if (exists)
            return ApiResponseDto<AuthResponseDto>.Fail(
                "An account with this email already exists.");

        var user = new User
        {
            FirstName    = dto.FirstName,
            LastName     = dto.LastName,
            Email        = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber  = dto.PhoneNumber,
            Role         = UserRole.Employee, // Default role on self-register
            IsActive     = true
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user,
            "Registration successful. Welcome to EMS!");
    }

    public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        // Always use constant-time comparison for security
        var user = await _uow.Users.GetByEmailAsync(dto.Email);

        // Important: same error for wrong email AND wrong password
        // Never tell attackers which one is wrong
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ApiResponseDto<AuthResponseDto>.Fail(
                "Invalid email or password.");

        if (!user.IsActive)
            return ApiResponseDto<AuthResponseDto>.Fail(
                "Your account has been deactivated. Contact HR.");

        return await GenerateAuthResponseAsync(user, "Login successful.");
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
    {
        // Step 1: Extract userId from the EXPIRED access token
        int userId;
        try
        {
            userId = _tokenService.GetUserIdFromExpiredToken(dto.AccessToken);
        }
        catch
        {
            return ApiResponseDto<AuthResponseDto>.Fail("Invalid access token.");
        }

        // Step 2: Find user and validate refresh token
        var user = await _uow.Users.GetByIdAsync(userId);

        if (user == null)
            return ApiResponseDto<AuthResponseDto>.Fail("User not found.");

        // Step 3: Validate refresh token matches AND hasn't expired
        if (user.RefreshToken != dto.RefreshToken)
            return ApiResponseDto<AuthResponseDto>.Fail("Invalid refresh token.");

        if (user.RefreshTokenExpiry <= DateTime.UtcNow)
            return ApiResponseDto<AuthResponseDto>.Fail(
                "Refresh token expired. Please login again.");

        // Step 4: Issue brand new token pair (rotation)
        return await GenerateAuthResponseAsync(user, "Token refreshed successfully.");
    }

    public async Task<ApiResponseDto<bool>> LogoutAsync(int userId)
    {
        var user = await _uow.Users.GetByIdAsync(userId);
        if (user == null)
            return ApiResponseDto<bool>.Fail("User not found.");

        // Invalidate refresh token in DB — this is what makes logout real
        // Even if someone steals the refresh token, it won't work after logout
        user.RefreshToken       = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt          = DateTime.UtcNow;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        return ApiResponseDto<bool>.Ok(true, "Logged out successfully.");
    }

    // ── Private helper ────────────────────────────────────────────────────────
    private async Task<ApiResponseDto<AuthResponseDto>> GenerateAuthResponseAsync(
        User user, string message)
    {
        var accessToken  = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Persist refresh token — this is what enables server-side invalidation
        user.RefreshToken       = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays);
        user.UpdatedAt          = DateTime.UtcNow;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync();

        var response = new AuthResponseDto
        {
            AccessToken         = accessToken,
            RefreshToken        = refreshToken,
            AccessTokenExpiry   = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            User = new UserInfoDto
            {
                Id              = user.Id,
                FullName        = user.FullName,
                Email           = user.Email,
                Role            = user.Role.ToString(),
                ProfilePhotoPath= user.ProfilePhotoPath
            }
        };

        return ApiResponseDto<AuthResponseDto>.Ok(response, message);
    }
}