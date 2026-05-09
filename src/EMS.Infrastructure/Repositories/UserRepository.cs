using EMS.Application.Interfaces.Repositories;
using EMS.Domain.Entities;
using EMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMS.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        => await _dbSet
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

    public async Task<IEnumerable<User>> GetAllWithEmployeeAsync()
        => await _dbSet
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Department)
            .ToListAsync();

    public async Task<User?> GetByIdWithEmployeeAsync(int id)
        => await _dbSet
            .Include(u => u.Employee)
                .ThenInclude(e => e!.Department)
            .FirstOrDefaultAsync(u => u.Id == id);
}