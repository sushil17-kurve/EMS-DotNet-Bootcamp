using EMS.Domain.Common;

namespace EMS.Domain.Entities;

public class LeaveType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int MaxDaysAllowed { get; set; }
    public string? Description { get; set; }
}