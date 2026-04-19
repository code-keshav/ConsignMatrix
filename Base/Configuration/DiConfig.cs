using Base.Helpers;
using Base.Helpers.Interfaces;
using Base.Manager;
using Base.Manager.Interface;
using Base.Providers;
using Base.Providers.Interfaces;
using Base.Repo;
using Base.Repo.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services;
using Base.Services.Consignment;
using Base.Services.Consignment.Interfaces;
using Base.Services.Interfaces;
using Base.Validator;
using Base.Validator.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace Base.Configuration;

public static class DiConfig
{
    public static IServiceCollection ConfigureBase(this IServiceCollection service) =>
        service.AddRepository().AddService().AddExtra();

    private static IServiceCollection AddRepository(this IServiceCollection service) =>
        service.AddTransient<IUow, Uow>()
            .AddTransient<IUserRepo, UserRepo>()
            .AddTransient<IBranchRepo, BranchRepo>()
            .AddTransient<IOrganizationRepo, OrganizationRepo>()
            .AddTransient<IRoleRepo, RoleRepo>()
            .AddTransient<IUserRoleRepo, UserRoleRepo>()
            .AddTransient<IUserBranchTransferRepo, UserBranchTransferRepo>()
            .AddTransient<IBranchPinCodeRepo, BranchPinCodeRepo>()
            .AddTransient<IEmployeeRepo, EmployeeRepo>()
            .AddTransient<IDriverRepo, DriverRepo>()
            .AddTransient<IVehicleRepo, VehicleRepo>()
            .AddTransient<IVehicleAssignmentRepo, VehicleAssignmentRepo>()
            .AddTransient<ICustomerRepo, CustomerRepo>()
            .AddTransient<ICustomerAddressRepo, CustomerAddressRepo>()
            .AddTransient<IConsignmentRepo, ConsignmentRepo>()
            .AddTransient<IPackageRepo, PackageRepo>()
            .AddTransient<IConsignmentStatusLogRepo, ConsignmentStatusLogRepo>()
            .AddTransient<IPickupTaskRepo, PickupTaskRepo>()
            .AddTransient<ITripRepo, TripRepo>()
            .AddTransient<ITripConsignmentRepo, TripConsignmentRepo>();

    private static IServiceCollection AddService(this IServiceCollection service) =>
        service.AddTransient<IAuthService, AuthService>()
            .AddTransient<IUserService, UserService>()
            .AddTransient<IBranchService, BranchService>()
            .AddTransient<IOrganizationService, OrganizationService>()
            .AddTransient<IRoleService, RoleService>()
            .AddTransient<IUserRoleService, UserRoleService>()
            .AddTransient<IUserBranchTransferService, UserBranchTransferService>()
            .AddTransient<IBranchPinCodeService, BranchPinCodeService>()
            .AddTransient<IEmployeeService, EmployeeService>()
            .AddTransient<IVehicleService, VehicleService>()
            .AddTransient<IDriverService, DriverService>()
            .AddTransient<IVehicleAssignmentService, VehicleAssignmentService>()
            .AddTransient<ICustomerService, CustomerService>()
            .AddTransient<ICustomerAddressService, CustomerAddressService>()
            .AddTransient<IConsignmentService, ConsignmentService>()
            .AddTransient<IPackageService, PackageService>()
            .AddTransient<IConsignmentStatusLogService, ConsignmentStatusLogService>()
            .AddTransient<IPickupTaskService, PickupTaskService>()
            .AddTransient<IBranchOperationService, BranchOperationService>()
            .AddTransient<ITripService, TripService>()
            .AddTransient<IDashboardService, DashboardService>();

    private static IServiceCollection AddExtra(this IServiceCollection service) =>
        service.AddScoped<ICurrentUserProvider, CurrentUserProvider>()
            .AddTransient<IFileHelper, FileHelper>()
            .AddTransient<IContentPathProvider, ContentPathProvider>()
            .AddTransient<IBranchManager, BranchManager>()
            .AddTransient<IUserValidator, UserValidator>()
            .AddTransient<IRoleValidator, RoleValidator>()
            .AddTransient<IUserBranchTransferActionValidator, UserBranchTransferActionValidator>()
            .AddTransient<IEmployeeManager, EmployeeManager>()
            .AddTransient<IEmployeeValidator, EmployeeValidator>()
            .AddTransient<IVehicleValidator, VehicleValidator>()
            .AddTransient<IDriverValidator, DriverValidator>()
            .AddTransient<IVehicleAssignmentValidator, VehicleAssignmentValidator>();
}