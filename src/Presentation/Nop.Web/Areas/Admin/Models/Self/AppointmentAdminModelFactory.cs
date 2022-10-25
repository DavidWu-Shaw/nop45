using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Self;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Cache;
using Nop.Web.Areas.Admin.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Self
{
    public partial class AppointmentAdminModelFactory : IAppointmentAdminModelFactory
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStaticCacheManager _cacheManager;

        public AppointmentAdminModelFactory(IProductService productService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IStaticCacheManager cacheManager)
        {
            _productService = productService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _cacheManager = cacheManager;
        }

        public virtual async Task<AppointmentEditModel> PrepareAppointmentEditModelAsync(Appointment appointment)
        {
            var model = new AppointmentEditModel();
            if (appointment != null)
            {
                var product = await _productService.GetProductByIdAsync(appointment.ResourceId);
                model.Id = appointment.Id;
                model.ResourceName = product.Name;
                model.ResourceId = appointment.ResourceId;
                var start = _dateTimeHelper.ConvertToUserTime(appointment.StartTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local);
                var end = _dateTimeHelper.ConvertToUserTime(appointment.EndTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local);
                model.TimeSlot = $"{start.ToShortTimeString()} - {end.ToShortTimeString()}, {start.ToShortDateString()} {start.ToString("dddd")}";
                model.Status = appointment.Status.ToString();
                model.Notes = appointment.Notes;

                if (appointment.CustomerId.HasValue)
                {
                    var customer = await _customerService.GetCustomerByIdAsync(appointment.CustomerId.Value);
                    model.CustomerId = appointment.CustomerId ?? 0;
                    model.CustomerFullName = await _customerService.GetCustomerFullNameAsync(customer);
                    model.CustomerEmail = customer.Email;
                }
            }

            return model;
        }

        public virtual async Task<AppointmentInfoModel> PrepareAppointmentInfoModel(Appointment appointment)
        {
            var model = new AppointmentInfoModel
            {
                id = appointment.Id.ToString(),
                start = _dateTimeHelper.ConvertToUserTime(appointment.StartTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss"),
                end = _dateTimeHelper.ConvertToUserTime(appointment.EndTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss"),
                resource = appointment.ResourceId.ToString()
            };
            var product = await _productService.GetProductByIdAsync(appointment.ResourceId);

            model.tags = new TagModel
            {
                status = appointment.Status.ToString(),
                doctor = product.Name
            };
            if (appointment.CustomerId.HasValue)
            {
                var customer = await _customerService.GetCustomerByIdAsync(appointment.CustomerId.Value);
                var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
                model.text = customerFullName ?? customer.Email;
            };

            return model;
        }

        public async Task<ProductCalendarModel> PrepareProductCalendarModel(ProductCalendarModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Id = product.Id;
            model.ProductName = product.Name;
            model.IsParentProduct = product.ProductType == ProductType.GroupedProduct;
            model.ShowCalendar = true;
            // Child product or parent product can't edit schedule
            model.ShowSchedule = product.ParentGroupedProductId == 0 && product.ProductType == ProductType.SimpleProduct;

            model.BusinessBeginsHour = 9;
            model.BusinessEndsHour = 23;
            model.BusinessMorningShiftEndsHour = 12;
            model.BusinessAfternoonShiftBeginsHour = 13;
            model.BusinessOnWeekends = true;

            return model;
        }

        public virtual async Task<VendorAppointmentInfoModel> PrepareVendorAppointmentInfoModel(Appointment appointment)
        {
            var model = new VendorAppointmentInfoModel
            {
                id = appointment.Id.ToString(),
                start = _dateTimeHelper.ConvertToUserTime(appointment.StartTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss"),
                end = _dateTimeHelper.ConvertToUserTime(appointment.EndTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss"),
                resource = appointment.ResourceId.ToString()
            };
            if (appointment.CustomerId.HasValue)
            {
                var customer = await _customerService.GetCustomerByIdAsync(appointment.CustomerId.Value);
                var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);
                model.text = customerFullName ?? customer.Email;
            };

            return model;
        }

        public virtual async Task<List<VendorResourceModel>> PrepareVendorResourcesModelAsync(int parentProductId)
        {
            var associatedProducts = await _productService.GetAssociatedProductsAsync(parentProductId);
            var model = new List<VendorResourceModel>();
            foreach (var product in associatedProducts)
            {
                model.Add(new VendorResourceModel { id = product.Id.ToString(), name = product.Name });
            }

            return model;
        }
    }
}
