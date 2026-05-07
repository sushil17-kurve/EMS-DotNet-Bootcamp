using EMS.Domain.Entities;
using EMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EmployeeCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.EmployeeCode)
            .IsUnique();

        builder.Property(e => e.Designation)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.Salary)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Address)
            .HasMaxLength(500);

        builder.Property(e => e.EmploymentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Employee → LeaveRequests
        builder.HasMany(e => e.LeaveRequests)
            .WithOne(l => l.Employee)
            .HasForeignKey(l => l.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}