using Base.Dtos.Consignment;
using Base.Entities.Consignment;

namespace Base.Services.Consignment.Interfaces;

public interface IPackageService
{
    Task Create(long consignmentId, PackageDto dto);
    Task Update(Package package, PackageDto dto);
    Task Delete(Package package);
}
