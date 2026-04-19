using Base.Dtos.Consignment;
using Base.Entities;
using Base.Enum;
using Base.Enum.Consignment;
using Base.Providers.Interfaces;
using Base.Repo.Consignment.Interfaces;
using Base.Repo.Interfaces;
using Base.Services.Consignment.Interfaces;
using Base.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Areas.Consignment.ViewModels;
using Web.Areas.Consignment.ViewModels.Consignment;
using Web.Areas.Consignment.ViewModels.Package;
using Web.Helpers;

namespace Web.Areas.Consignment.Controllers;

[Area("Consignment")]
public class ConsignmentController : Controller
{
    private readonly INotificationHelper _notificationHelper;
    private readonly IConsignmentService _consignmentService;
    private readonly IConsignmentRepo _consignmentRepo;
    private readonly IConsignmentStatusLogRepo _statusLogRepo;
    private readonly IPackageRepo _packageRepo;
    private readonly ICustomerRepo _customerRepo;
    private readonly ICustomerAddressRepo _customerAddressRepo;
    private readonly IBranchPinCodeService _branchPinCodeService;
    private readonly IBranchRepo _branchRepo;
    private readonly ICurrentUserProvider _currentUserProvider;

    public ConsignmentController(
        INotificationHelper notificationHelper,
        IConsignmentService consignmentService,
        IConsignmentRepo consignmentRepo,
        IConsignmentStatusLogRepo statusLogRepo,
        IPackageRepo packageRepo,
        ICustomerRepo customerRepo,
        ICustomerAddressRepo customerAddressRepo,
        IBranchPinCodeService branchPinCodeService,
        IBranchRepo branchRepo, ICurrentUserProvider currentUserProvider)
    {
        _notificationHelper = notificationHelper;
        _consignmentService = consignmentService;
        _consignmentRepo = consignmentRepo;
        _statusLogRepo = statusLogRepo;
        _packageRepo = packageRepo;
        _customerRepo = customerRepo;
        _customerAddressRepo = customerAddressRepo;
        _branchPinCodeService = branchPinCodeService;
        _branchRepo = branchRepo;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? trackingNumber, ServiceType? serviceType, PaymentMode? paymentMode, DateTime? dateFrom, DateTime? dateTo)
    {
        try
        {
            var queryable = _consignmentRepo.GetQueryable();

            if (!string.IsNullOrWhiteSpace(trackingNumber))
            {
                var search = trackingNumber.Trim().ToLower();
                queryable = queryable.Where(c => c.TrackingNumber.ToLower().Contains(search));
            }

            if (serviceType != null)
                queryable = queryable.Where(c => c.ServiceType == serviceType);

            if (paymentMode != null)
                queryable = queryable.Where(c => c.PaymentMode == paymentMode);

            if (dateFrom != null)
                queryable = queryable.Where(c => c.RecDate >= dateFrom.Value.ToUniversalTime());

            if (dateTo != null)
                queryable = queryable.Where(c => c.RecDate <= dateTo.Value.Date.AddDays(1).ToUniversalTime());

            var consignments = await queryable.OrderByDescending(c => c.RecDate).ToListAsync();

            var vm = new ConsignmentFilterVm
            {
                TrackingNumber = trackingNumber,
                ServiceType = serviceType,
                PaymentMode = paymentMode,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Consignments = consignments.Select(c =>
                {
                    var latestLog = c.StatusLogs?.OrderByDescending(s => s.RecDate).FirstOrDefault();
                    return new ConsignmentListItemVm
                    {
                        Id = c.Id,
                        TrackingNumber = c.TrackingNumber,
                        SenderName = c.Sender?.Name ?? "",
                        ReceiverName = c.Receiver?.Name ?? "",
                        ServiceType = c.ServiceType,
                        PaymentMode = c.PaymentMode,
                        PackageCount = c.PackageCount,
                        ChargeableWeight = c.ChargeableWeight,
                        LatestStatus = latestLog?.StatusType.ToString(),
                        RecDate = c.RecDate,
                        IsActive = c.Status == StatusEnum.Active,
                    };
                }).ToList()
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ConsignmentCreateVm());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ConsignmentCreateVm vm)
    {
        try
        {
            var cu = await _currentUserProvider.GetCurrentUser();
            var dto = new ConsignmentDto
            {
                SenderId = vm.SenderId,
                SenderAddressId = vm.SenderAddressId,
                ReceiverId = vm.ReceiverId,
                ReceiverAddressId = vm.ReceiverAddressId,
                DestinationBranchId = vm.DestinationBranchId,
                ServiceType = vm.ServiceType,
                PaymentMode = vm.PaymentMode,
                DeclaredValue = vm.DeclaredValue,
                CodAmount = vm.CodAmount,
                SpecialInstructions = vm.SpecialInstructions,
                BranchId = cu.BranchId,
                Packages = vm.Packages.Select(p => new PackageDto
                {
                    Weight = p.Weight,
                    Length = p.Length,
                    Width = p.Width,
                    Height = p.Height,
                    PackageType = p.PackageType,
                    ContentDescription = p.ContentDescription,
                    IsFragile = p.IsFragile,
                    IsHazardous = p.IsHazardous,
                    CanBeStacked = p.CanBeStacked,
                    RequiresColdChain = p.RequiresColdChain,
                }).ToList()
            };

            // Handle sender: new customer OR new address for existing customer
            if (!vm.SenderId.HasValue && vm.SenderName != null)
            {
                dto.NewSender = new CustomerDto
                {
                    Name = vm.SenderName,
                    PhoneNo = vm.SenderPhone!,
                    Email = vm.SenderEmail,
                    CustomerType = vm.SenderCustomerType ?? CustomerType.Individual,
                    AddressDto = new CustomerAddressDto
                    {
                        AddressType = vm.NewSenderAddress?.AddressType ?? AddressType.Home,
                        AddressLine1 = vm.NewSenderAddress?.AddressLine1 ?? "",
                        City = vm.NewSenderAddress?.City ?? "",
                        State = vm.NewSenderAddress?.State ?? "",
                        PinCode = vm.NewSenderAddress?.PinCode ?? "",
                        AddressLine2 = vm.NewSenderAddress?.AddressLine2,
                        Landmark = vm.NewSenderAddress?.Landmark,
                        Latitude = vm.NewSenderAddress?.Latitude,
                        Longitude = vm.NewSenderAddress?.Longitude,
                        ContactNo = vm.NewSenderAddress?.ContactNo,
                        IsDefault = true,
                    }
                };
            }
            else if (vm.SenderId.HasValue && !vm.SenderAddressId.HasValue && vm.NewSenderAddress != null)
            {
                dto.NewSenderAddress = new CustomerAddressDto
                {
                    CustomerId = vm.SenderId.Value,
                    AddressType = vm.NewSenderAddress.AddressType,
                    AddressLine1 = vm.NewSenderAddress.AddressLine1 ?? "",
                    City = vm.NewSenderAddress.City ?? "",
                    State = vm.NewSenderAddress.State ?? "",
                    PinCode = vm.NewSenderAddress.PinCode ?? "",
                    AddressLine2 = vm.NewSenderAddress.AddressLine2,
                    Landmark = vm.NewSenderAddress.Landmark,
                    Latitude = vm.NewSenderAddress.Latitude,
                    Longitude = vm.NewSenderAddress.Longitude,
                    ContactNo = vm.NewSenderAddress.ContactNo,
                    IsDefault = false,
                };
            }

            // Handle receiver: new customer OR new address for existing customer
            if (!vm.ReceiverId.HasValue && vm.ReceiverName != null)
            {
                dto.NewReceiver = new CustomerDto
                {
                    Name = vm.ReceiverName,
                    PhoneNo = vm.ReceiverPhone!,
                    Email = vm.ReceiverEmail,
                    CustomerType = vm.ReceiverCustomerType ?? CustomerType.Individual,
                    AddressDto = new CustomerAddressDto
                    {
                        AddressType = vm.NewReceiverAddress?.AddressType ?? AddressType.Home,
                        AddressLine1 = vm.NewReceiverAddress?.AddressLine1 ?? "",
                        City = vm.NewReceiverAddress?.City ?? "",
                        State = vm.NewReceiverAddress?.State ?? "",
                        PinCode = vm.NewReceiverAddress?.PinCode ?? "",
                        AddressLine2 = vm.NewReceiverAddress?.AddressLine2,
                        Landmark = vm.NewReceiverAddress?.Landmark,
                        Latitude = vm.NewReceiverAddress?.Latitude,
                        Longitude = vm.NewReceiverAddress?.Longitude,
                        ContactNo = vm.NewReceiverAddress?.ContactNo,
                        IsDefault = true,
                    }
                };
            }
            else if (vm.ReceiverId.HasValue && !vm.ReceiverAddressId.HasValue && vm.NewReceiverAddress != null)
            {
                dto.NewReceiverAddress = new CustomerAddressDto
                {
                    CustomerId = vm.ReceiverId.Value,
                    AddressType = vm.NewReceiverAddress.AddressType,
                    AddressLine1 = vm.NewReceiverAddress.AddressLine1 ?? "",
                    City = vm.NewReceiverAddress.City ?? "",
                    State = vm.NewReceiverAddress.State ?? "",
                    PinCode = vm.NewReceiverAddress.PinCode ?? "",
                    AddressLine2 = vm.NewReceiverAddress.AddressLine2,
                    Landmark = vm.NewReceiverAddress.Landmark,
                    Latitude = vm.NewReceiverAddress.Latitude,
                    Longitude = vm.NewReceiverAddress.Longitude,
                    ContactNo = vm.NewReceiverAddress.ContactNo,
                    IsDefault = false,
                };
            }

            await _consignmentService.Create(dto);
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(long id)
    {
        try
        {
            var consignment = await _consignmentRepo.FindOrThrowAsync(id);
            var senderAddress = consignment.SenderAddress;
            var receiverAddress = consignment.ReceiverAddress;
            var vm = new ConsignmentVm
            {
                Id = consignment.Id,
                TrackingNumber = consignment.TrackingNumber,
                ServiceType = consignment.ServiceType,
                PaymentMode = consignment.PaymentMode,
                DeclaredValue = consignment.DeclaredValue,
                CodAmount = consignment.CodAmount,
                SpecialInstructions = consignment.SpecialInstructions,
                DestinationBranchId = consignment.DestinationBranchId,
                DestinationBranchName = consignment.DestinationBranch?.Name,
                ReceiverPinCode = receiverAddress?.PinCode,
                SenderId = consignment.SenderId,
                SenderName = consignment.Sender?.Name,
                SenderPhone = consignment.Sender?.PhoneNo,
                SenderAddressId = consignment.SenderAddressId,
                SenderAddressDisplay = senderAddress != null
                    ? $"{senderAddress.AddressLine1}, {senderAddress.City}, {senderAddress.State} - {senderAddress.PinCode}"
                    : null,
                ReceiverId = consignment.ReceiverId,
                ReceiverName = consignment.Receiver?.Name,
                ReceiverPhone = consignment.Receiver?.PhoneNo,
                ReceiverAddressId = consignment.ReceiverAddressId,
                ReceiverAddressDisplay = receiverAddress != null
                    ? $"{receiverAddress.AddressLine1}, {receiverAddress.City}, {receiverAddress.State} - {receiverAddress.PinCode}"
                    : null,
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(ConsignmentVm vm)
    {
        try
        {
            var consignment = await _consignmentRepo.FindOrThrowAsync(vm.Id);
            var dto = new ConsignmentUpdateDto
            {
                ServiceType = vm.ServiceType,
                PaymentMode = vm.PaymentMode,
                DeclaredValue = vm.DeclaredValue,
                CodAmount = vm.CodAmount,
                SpecialInstructions = vm.SpecialInstructions,
                DestinationBranchId = vm.DestinationBranchId > 0 ? vm.DestinationBranchId : null,
            };
            await _consignmentService.Update(consignment, dto);
            return RedirectToAction(nameof(Detail), new { id = vm.Id });
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        try
        {
            var consignment = await _consignmentRepo.FindOrThrowAsync(id);
            var packages = await _packageRepo.GetByConsignmentId(id);
            var statusLogs = await _statusLogRepo.GetByConsignmentId(id);
            var latestStatus = statusLogs.FirstOrDefault();

            var senderAddress = consignment.SenderAddress;
            var receiverAddress = consignment.ReceiverAddress;

            var vm = new ConsignmentDetailVm
            {
                Id = consignment.Id,
                TrackingNumber = consignment.TrackingNumber,
                SenderName = consignment.Sender?.Name ?? "",
                SenderPhone = consignment.Sender?.PhoneNo ?? "",
                SenderAddress = senderAddress != null
                    ? $"{senderAddress.AddressLine1}, {senderAddress.City}, {senderAddress.State} - {senderAddress.PinCode}"
                    : "",
                ReceiverName = consignment.Receiver?.Name ?? "",
                ReceiverPhone = consignment.Receiver?.PhoneNo ?? "",
                ReceiverAddress = receiverAddress != null
                    ? $"{receiverAddress.AddressLine1}, {receiverAddress.City}, {receiverAddress.State} - {receiverAddress.PinCode}"
                    : "",
                OriginBranchName = consignment.OriginBranch?.Name ?? "",
                DestinationBranchName = consignment.DestinationBranch?.Name ?? "",
                ServiceType = consignment.ServiceType,
                PaymentMode = consignment.PaymentMode,
                DeclaredValue = consignment.DeclaredValue,
                CodAmount = consignment.CodAmount,
                SpecialInstructions = consignment.SpecialInstructions,
                TotalWeight = consignment.TotalWeight,
                VolumetricWeight = consignment.VolumetricWeight,
                ChargeableWeight = consignment.ChargeableWeight,
                TotalVolume = consignment.TotalVolume,
                PackageCount = consignment.PackageCount,
                ExpectedDeliveryDate = consignment.ExpectedDeliveryDate,
                ActualDeliveryDate = consignment.ActualDeliveryDate,
                RecDate = consignment.RecDate,
                LatestStatus = latestStatus?.StatusType.ToString(),
                IsActive = consignment.Status == StatusEnum.Active,
                Packages = packages.Select(p => new PackageVm
                {
                    Id = p.Id,
                    ConsignmentId = p.ConsignmentId,
                    PackageNumber = p.PackageNumber,
                    Barcode = p.Barcode,
                    Weight = p.Weight,
                    Length = p.Length,
                    Width = p.Width,
                    Height = p.Height,
                    Volume = p.Volume,
                    VolumetricWeight = p.VolumetricWeight,
                    PackageType = p.PackageType,
                    ContentDescription = p.ContentDescription,
                    IsFragile = p.IsFragile,
                    IsHazardous = p.IsHazardous,
                    CanBeStacked = p.CanBeStacked,
                    RequiresColdChain = p.RequiresColdChain,
                }).ToList(),
                StatusLogs = statusLogs.Select(s => new ConsignmentStatusLogVm
                {
                    StatusType = s.StatusType,
                    Remarks = s.Remarks,
                    RecDate = s.RecDate,
                    RecByName = s.RecBy?.Name,
                }).ToList(),
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var consignment = await _consignmentRepo.FindOrThrowAsync(id);
            await _consignmentService.Delete(consignment);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> SearchCustomer(string term)
    {
        try
        {
            var search = term.Trim().ToLower();
            var customers = await _customerRepo.FindByAsync(c =>
                c.Name.ToLower().Contains(search) || c.PhoneNo.Contains(search));
            return Json(customers.Select(c => new
            {
                c.Id,
                c.Name,
                c.PhoneNo,
                c.Email,
                c.CustomerType,
            }));
        }
        catch
        {
            return Json(new List<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomerAddresses(long customerId)
    {
        try
        {
            var addresses = await _customerAddressRepo.GetByCustomerId(customerId);
            return Json(addresses.Select(a => new
            {
                a.Id,
                a.AddressType,
                a.AddressLine1,
                a.AddressLine2,
                a.City,
                a.State,
                a.PinCode,
                a.Landmark,
                a.IsDefault,
            }));
        }
        catch
        {
            return Json(new List<object>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> CheckServiceability(string pinCode)
    {
        try
        {
            var pinMatched = await _branchPinCodeService.CheckServiceability(pinCode);
            if (pinMatched.Count > 0)
            {
                return Json(new
                {
                    serviceable = true,
                    branches = pinMatched.Select(b => new
                    {
                        id = b.Id,
                        name = b.Name,
                        code = b.Code,
                        currentLoad = b.CurrentLoad,
                        storageCapacity = b.StorageCapacity,
                    }).ToList()
                });
            }

            // No pin code match — return all active branches as fallback
            var allBranches = await _branchRepo.FindByAsync(b => b.Status == StatusEnum.Active);
            return Json(new
            {
                serviceable = false,
                branches = allBranches.Select(b => new
                {
                    id = b.Id,
                    name = b.Name,
                    code = b.Code,
                    currentLoad = b.CurrentLoad,
                    storageCapacity = b.StorageCapacity,
                }).ToList()
            });
        }
        catch
        {
            return Json(new { serviceable = false, branches = new List<object>() });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBranches()
    {
        try
        {
            var branches = await _branchRepo.FindByAsync(b => b.Status == StatusEnum.Active);
            return Json(branches.Select(b => new
            {
                id = b.Id,
                name = b.Name,
                code = b.Code,
                currentLoad = b.CurrentLoad,
                storageCapacity = b.StorageCapacity,
            }).ToList());
        }
        catch
        {
            return Json(new List<object>());
        }
    }
}
