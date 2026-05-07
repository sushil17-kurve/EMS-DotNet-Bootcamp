using EMS.Domain.Entities;
using EMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EMS.Infrastructure.Data.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(lr => lr.Id);

        builder.Property(lr => lr.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(lr => lr.ReviewNote)
            .HasMaxLength(500);

        builder.Property(lr => lr.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // LeaveRequest → LeaveType (no cascade, LeaveType is reference data)
        builder.HasOne(lr => lr.LeaveType)
            .WithMany()
            .HasForeignKey(lr => lr.LeaveTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ⚠️ THIS solves the "multiple cascade paths" homework problem from Day 1
        // ReviewedBy is a User, Employee is also linked to User
        // SQL Server can't have two cascade paths to the same table
        builder.HasOne(lr => lr.ReviewedBy)
            .WithMany()
            .HasForeignKey(lr => lr.ReviewedById)
            .OnDelete(DeleteBehavior.NoAction);  // ← Solution!
    }
}