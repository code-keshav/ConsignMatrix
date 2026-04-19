using Base.Dtos.Consignment;

namespace Base.Services.Consignment.Interfaces;

public interface IDashboardService
{
    Task<DashboardData> GetDashboardDataAsync(long? branchId, int daysBack = 30);
}
