using EMS.Application.Interfaces;
using EMS.Application.Interfaces.Repositories;
using EMS.Infrastructure.Data;
using EMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace EMS.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy initialization — repositories created only when first accessed
    private IUserRepository?         _users;
    private IEmployeeRepository?     _employees;
    private IDepartmentRepository?   _departments;
    private ILeaveRequestRepository? _leaveRequests;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // ??= means: "if null, create it; otherwise return existing"
    // All repositories SHARE the same DbContext = same transaction
    public IUserRepository         Users
        => _users         ??= new UserRepository(_context);

    public IEmployeeRepository     Employees
        => _employees     ??= new EmployeeRepository(_context);

    public IDepartmentRepository   Departments
        => _departments   ??= new DepartmentRepository(_context);

    public ILeaveRequestRepository LeaveRequests
        => _leaveRequests ??= new LeaveRequestRepository(_context);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}