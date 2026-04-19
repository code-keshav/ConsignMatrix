using Base.Dtos.Consignment;
using Base.Repo.Consignment.Interfaces;
using Base.Services.Consignment.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Web.Areas.Consignment.ViewModels;
using Web.Helpers;

namespace Web.Areas.Consignment.Controllers;

[Area("Consignment")]
public class CustomerAddressController : Controller
{
    private readonly ICustomerRepo _customerRepo;
    private readonly INotificationHelper _notificationHelper;
    private readonly ICustomerAddressRepo _customerAddressRepo;
    private readonly ICustomerAddressService _customerAddressService;

    public CustomerAddressController(ICustomerRepo customerRepo, INotificationHelper notificationHelper, ICustomerAddressRepo customerAddressRepo, ICustomerAddressService customerAddressService)
    {
        _customerRepo = customerRepo;
        _notificationHelper = notificationHelper;
        _customerAddressRepo = customerAddressRepo;
        _customerAddressService = customerAddressService;
    }

    [HttpGet]
    public async Task<IActionResult> AddAddress(long id)
    {
        try
        {
            var customer = await _customerRepo.FindOrThrowAsync(id);
            var vm = new CustomerAddressVm
            {
                CustomerId = id,
                CustomerName = customer.Name
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction("Detail", "Customer", new { id });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddAddress(CustomerAddressVm vm)
    {
        try
        {
            var customer = await _customerRepo.FindOrThrowAsync(vm.CustomerId);
            if (await _customerAddressRepo.CheckIfExistAsync(a => a.CustomerId == customer.Id && a.AddressType == vm.AddressType))
            {
                throw new Exception($"The address type {vm.AddressType} is already exists. Please update the existing address if you wish to update it.");
            }

            var dto = new CustomerAddressDto
            {
                CustomerId = customer.Id,
                AddressType = vm.AddressType,
                AddressLine1 = vm.AddressLine1,
                AddressLine2 = vm.AddressLine2,
                City = vm.City,
                State = vm.State,
                PinCode = vm.PinCode,
                Landmark = vm.Landmark,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                ContactNo = vm.ContactNo,
                IsDefault = false
            };
            await _customerAddressService.Create(dto);
            return View(vm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction("Detail", "Customer", new { id = vm.CustomerId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAddress(long id)
    {
        try
        {
            var address = await _customerAddressRepo.FindOrThrowAsync(id);
            return Json(new
            {
                id = address.Id,
                customerId = address.CustomerId,
                addressType = (int)address.AddressType,
                addressLine1 = address.AddressLine1,
                addressLine2 = address.AddressLine2,
                city = address.City,
                state = address.State,
                pinCode = address.PinCode,
                landmark = address.Landmark,
                latitude = address.Latitude,
                longitude = address.Longitude,
                contactNo = address.ContactNo,
                isDefault = address.IsDefault,
            });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAjax([FromBody] CustomerAddressVm vm)
    {
        try
        {
            var address = await _customerAddressRepo.FindOrThrowAsync(vm.AddressId);
            var dto = new CustomerAddressDto
            {
                CustomerId = vm.CustomerId,
                AddressType = vm.AddressType,
                AddressLine1 = vm.AddressLine1,
                AddressLine2 = vm.AddressLine2,
                City = vm.City,
                State = vm.State,
                PinCode = vm.PinCode,
                Landmark = vm.Landmark,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                ContactNo = vm.ContactNo,
                IsDefault = address.IsDefault,
            };
            await _customerAddressService.Update(address, dto);
            return Json(new
            {
                success = true,
                data = new
                {
                    id = address.Id,
                    customerId = vm.CustomerId,
                    addressType = (int)vm.AddressType,
                    addressLine1 = vm.AddressLine1,
                    addressLine2 = vm.AddressLine2,
                    city = vm.City,
                    state = vm.State,
                    pinCode = vm.PinCode,
                    landmark = vm.Landmark,
                    contactNo = vm.ContactNo,
                },
                message = "Address updated successfully."
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Update(long id, long customerId)
    {
        try
        {
            var address = await _customerAddressRepo.FindOrThrowAsync(id);
            var customerAddressVm = new CustomerAddressVm
            {
                CustomerId = customerId,
                AddressId = address.Id,
                AddressType = address.AddressType,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                City = address.City,
                State = address.State,
                PinCode = address.PinCode,
                Landmark = address.Landmark,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                ContactNo = address.ContactNo,
                CustomerName = address.Customer.Name
            };
            return View(customerAddressVm);
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction("Detail", "Customer", new { id = customerId });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Update(CustomerAddressVm vm)
    {
        try
        {
            var address = await _customerAddressRepo.FindOrThrowAsync(vm.AddressId);
            var dto = new CustomerAddressDto
            {
                CustomerId = vm.CustomerId,
                AddressType = vm.AddressType,
                AddressLine1 = vm.AddressLine1,
                AddressLine2 = vm.AddressLine2,
                City = vm.City,
                State = vm.State,
                PinCode = vm.PinCode,
                Landmark = vm.Landmark,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                ContactNo = vm.ContactNo,
                IsDefault = false,
            };
            await _customerAddressService.Update(address, dto);
            return RedirectToAction("Detail", "Customer", new { id = vm.CustomerId });
        }
        catch (Exception ex)
        {
            _notificationHelper.SetErrorMsg(ex.Message);
            return RedirectToAction("Update", new { id = vm.AddressId, customerId = vm.CustomerId });
        }
    }
}