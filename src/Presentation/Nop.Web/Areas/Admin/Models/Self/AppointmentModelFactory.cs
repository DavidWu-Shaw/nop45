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
    public partial class AppointmentModelFactory : IAppointmentModelFactory
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStaticCacheManager _cacheManager;

        public AppointmentModelFactory(IProductService productService, 
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
                model.Id = appointment.Id;
                model.ResourceName = appointment.Product.Name;
                model.ResourceId = appointment.ResourceId;
                var start = _dateTimeHelper.ConvertToUserTime(appointment.StartTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local);
                var end = _dateTimeHelper.ConvertToUserTime(appointment.EndTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local);
                model.TimeSlot = $"{start.ToShortTimeString()} - {end.ToShortTimeString()}, {start.ToShortDateString()} {start.ToString("dddd")}";
                model.Status = appointment.Status.ToString();
                model.Notes = appointment.Notes;
                model.CustomerId = appointment.CustomerId ?? 0;
                if (appointment.Customer != null)
                {
                    model.CustomerFullName = await _customerService.GetCustomerFullNameAsync(appointment.Customer);
                    model.CustomerEmail = appointment.Customer.Email;
                }
            }

            return model;
        }

        public virtual AppointmentInfoModel PrepareAppointmentInfoModel(Appointment appointment)
        {
            var model = new AppointmentInfoModel
            {
                id = appointment.Id.ToString(),
                start = _dateTimeHelper.ConvertToUserTime(appointment.StartTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss"),
                end = _dateTimeHelper.ConvertToUserTime(appointment.EndTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss"),
                resource = appointment.ResourceId.ToString()
            };
            model.tags = new TagModel
            {
                status = appointment.Status.ToString(),
                doctor = appointment.Product.Name
            };
            if (appointment.Customer != null)
            {
                model.text = appointment.Customer.Username ?? appointment.Customer.Email;
            };

            return model;
        }

        public ProductCalendarModel PrepareProductCalendarModel(ProductCalendarModel model, Product product)
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

        public virtual VendorAppointmentInfoModel PrepareVendorAppointmentInfoModel(Appointment appointment)
        {
            var model = new VendorAppointmentInfoModel
            {
                id = appointment.Id.ToString(),
                start = _dateTimeHelper.ConvertToUserTime(appointment.StartTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss"),
                end = _dateTimeHelper.ConvertToUserTime(appointment.EndTimeUtc, TimeZoneInfo.Utc, TimeZoneInfo.Local).ToString("yyyy-MM-ddTHH:mm:ss"),
                resource = appointment.ResourceId.ToString()
            };
            if (appointment.Customer != null)
            {
                model.text = appointment.Customer.Username ?? appointment.Customer.Email;
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
