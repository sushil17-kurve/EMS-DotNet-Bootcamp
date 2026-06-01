using EMS.Application.DTOs.Auth;
using EMS.Application.DTOs.Common;

namespace EMS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
    Task<ApiResponseDto<bool>> LogoutAsync(int userId);
}