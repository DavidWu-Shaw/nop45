using Nop.Core.Caching;
using Nop.Core.Domain.Self;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using System;
using System.Collections.Generic;
using Nop.Web.Infrastructure.Cache;
using System.Threading.Tasks;

namespace Nop.Web.Models.Self
{
    public partial class AppointmentModelFactory : IAppointmentModelFactory
    {
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStaticCacheManager _cacheManager;

        public AppointmentModelFactory(IProductService productService,
            IStaticCacheManager cacheManager,
            IDateTimeHelper dateTimeHelper)
        {
            _productService = productService;
            _cacheManager = cacheManager;
            _dateTimeHelper = dateTimeHelper;
        }

        public virtual AppointmentUpdateModel PrepareAppointmentUpdateModel(Appointment appointment)
        {
            var model = new AppointmentUpdateModel();
            if (appointment != null)
            {
                model.Id = appointment.Id;
                var start = _dateTimeHelper.ConvertToUserTime(appointment.StartTimeUtc, TimeZoneInfo.Local, TimeZoneInfo.Utc);
                var end = _dateTimeHelper.ConvertToUserTime(appointment.EndTimeUtc, TimeZoneInfo.Local, TimeZoneInfo.Utc);
                model.TimeSlot = $"{start.ToShortTimeString()} - {end.ToShortTimeString()}, {start.ToShortDateString()} {start.ToString("dddd")}";
                model.Status = appointment.Status;
                model.Notes = appointment.Notes;
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
                model.text = appointment.Customer.Username;
            };

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

            //var cacheKey = string.Format(NopModelCacheDefaults.VendorProductsCacheKeyById, parentProductId);
            //var cachedModel = _cacheManager.Get(cacheKey, async () =>
            //{
            //    var associatedProducts = await _productService.GetAssociatedProductsAsync(parentProductId);
            //    var model = new List<VendorResourceModel>();
            //    foreach (var product in associatedProducts)
            //    {
            //        model.Add(new VendorResourceModel { id = product.Id.ToString(), name = product.Name });
            //    }

            //    return model;
            //});

            //return cachedModel;
        }
    }
}
