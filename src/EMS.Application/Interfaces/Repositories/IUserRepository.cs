using EMS.Domain.Entities;

namespace EMS.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetAllWithEmployeeAsync();
    Task<User?> GetByIdWithEmployeeAsync(int id);
}