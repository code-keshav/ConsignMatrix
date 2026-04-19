using Base.Dtos.Consignment;
using Base.Entities;
using Base.Enum.Consignment;
using Base.Extensions;
using Base.Repo.Consignment.Interfaces;
using Base.Services.Consignment.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Consignment.ViewModels;
using Web.Helpers;

namespace Web.Areas.Consignment.Controllers;

[Area("Consignment")]
public class CustomerController : Controller
{
    private readonly INotificationHelper _notificationHelper;
    private readonly ICustomerService _customerService;
    private readonly ICustomerRepo _customerRepo;
    private readonly ICustomerAddressRepo _customerAddressRepo;
    private readonly ICustomerAddressService _customerAddressService;

    public CustomerController(INotificationHelper notificationHelper, ICustomerService customerService, ICustomerRepo customerRepo, ICustomerAddressRepo customerAddressRepo, ICustomerAddressService customerAddressService)
    {
        _notificationHelper = notificationHelper;
        _customerService = customerService;
        _customerRepo = customerRepo;
        _customerAddressRepo = customerAddressRepo;
        _customerAddressService = customerAddressService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm, CustomerType? customerType)
    {
        try
        {
            var queryable = _customerRepo.GetQueryable();
            if (searchTerm != null)
            {
                var search = searchTerm.Trim().ToLower();
                queryable = queryable.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    (x.Email != null && x.Email.ToLower().Contains(search)) ||
                    x.PhoneNo.Contains(search));
            }

            if (customerType != null)
            {
                queryable = queryable.Where(a => a.CustomerType == customerType);
            }

            var vm = new CustomerFilterVm
            {
                CustomerType = customerType,
                SearchTerm = searchTerm,
                Customers = queryable.Select(a => new CustomerVm
                {
                    Id = a.Id,
                    Name = a.Name,
                    PhoneNo = a.PhoneNo,
                    SecondaryPhoneNo = a.SecondaryPhoneNo,
                    Email = a.Email,
                    CustomerType = a.CustomerType,
                    IsActive = a.Status == StatusEnum.Active
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
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CustomerCreateVm customerCreateVm)
    {
        try
        {
            if (!ModelState.IsValid)
                return View();
            var dto = new CustomerDto
            {
                Name = customerCreateVm.Name,
                PhoneNo = customerCreateVm.PhoneNo,
                SecondaryPhoneNo = customerCreateVm.SecondaryPhoneNo,
                Email = customerCreateVm.Email,
                CustomerType = customerCreateVm.CustomerType,
                AddressDto = new CustomerAddressDto
                {
                    AddressType = customerCreateVm.HomeAddress.AddressType,
                    AddressLine1 = customerCreateVm.HomeAddress.AddressLine1,
                    AddressLine2 = customerCreateVm.HomeAddress.AddressLine2,
                    City = customerCreateVm.HomeAddress.City,
                    State = customerCreateVm.HomeAddress.State,
                    PinCode = customerCreateVm.HomeAddress.PinCode,
                    Landmark = customerCreateVm.HomeAddress.Landmark,
                    Latitude = customerCreateVm.HomeAddress.Latitude,
                    Longitude = customerCreateVm.HomeAddress.Longitude,
                    ContactNo = customerCreateVm.HomeAddress.ContactNo,
                    IsDefault = true,
                }
            };
            await _customerService.Create(dto);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(long id)
    {
        try
        {
            var customer = await _customerRepo.FindOrThrowAsync(id);
            var vm = new CustomerVm
            {
                Id = customer.Id,
                Name = customer.Name,
                PhoneNo = customer.PhoneNo,
                SecondaryPhoneNo = customer.SecondaryPhoneNo,
                Email = customer.Email,
                CustomerType = customer.CustomerType,
                IsActive = customer.Status == StatusEnum.Active
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return View();
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Update(CustomerVm vm)
    {
        try
        {
            var customer = await _customerRepo.FindOrThrowAsync(vm.Id);
            var dto = new CustomerDto
            {
                Name = vm.Name,
                PhoneNo = vm.PhoneNo,
                SecondaryPhoneNo = vm.SecondaryPhoneNo,
                Email = vm.Email,
                CustomerType = vm.CustomerType,
            };
            await _customerService.Update(customer, dto);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return View();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Activate(long id)
    {
        try
        {
            var customer = await _customerRepo.FindOrThrowAsync(id);
            await _customerService.Activate(customer);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            var customer = await _customerRepo.FindOrThrowAsync(id);
            await _customerService.Deactivate(customer);
            return RedirectToAction(nameof(Index));
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
            var customer = await _customerRepo.FindOrThrowAsync(id);
            await _customerService.Delete(customer);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction(nameof(Index));
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetCustomer(long id)
    {
        try
        {
            var customer = await _customerRepo.FindOrThrowAsync(id);
            return Json(new
            {
                id = customer.Id,
                name = customer.Name,
                phoneNo = customer.PhoneNo,
                secondaryPhoneNo = customer.SecondaryPhoneNo,
                email = customer.Email,
                customerType = (int)customer.CustomerType,
            });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAjax([FromBody] CustomerVm vm)
    {
        try
        {
            var customer = await _customerRepo.FindOrThrowAsync(vm.Id);
            var dto = new CustomerDto
            {
                Name = vm.Name,
                PhoneNo = vm.PhoneNo,
                SecondaryPhoneNo = vm.SecondaryPhoneNo,
                Email = vm.Email,
                CustomerType = vm.CustomerType,
            };
            await _customerService.Update(customer, dto);
            return Json(new
            {
                success = true,
                data = new
                {
                    id = customer.Id,
                    name = vm.Name,
                    phoneNo = vm.PhoneNo,
                    secondaryPhoneNo = vm.SecondaryPhoneNo,
                    email = vm.Email,
                    customerType = (int)vm.CustomerType,
                },
                message = "Customer updated successfully."
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Detail(long id)
    {
        try
        {
            var customer = await _customerRepo.FindOrThrowAsync(id);
            var vm = new CustomerDetailVm
            {
                Name = customer.Name,
                PhoneNo = customer.PhoneNo,
                SecondaryPhoneNo = customer.SecondaryPhoneNo,
                Email = customer.Email,
                CustomerType = customer.CustomerType,
                Id = id,
                IsActive = customer.Status == StatusEnum.Active,
                Status = customer.Status.ToString(),
                AddressVms = customer.CustomerAddresses.Select(a => new CustomerAddressVm
                {
                    AddressId = a.Id,
                    CustomerId = id,
                    AddressType = a.AddressType,
                    AddressLine1 = a.AddressLine1,
                    AddressLine2 = a.AddressLine2,
                    City = a.City,
                    State = a.State,
                    PinCode = a.PinCode,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    ContactNo = a.ContactNo,
                    IsDefault = a.IsDefault
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
}