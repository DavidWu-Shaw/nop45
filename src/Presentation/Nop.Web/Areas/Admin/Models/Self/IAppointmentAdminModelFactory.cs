using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Self;
using Nop.Web.Areas.Admin.Models.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Self
{
    public interface IAppointmentAdminModelFactory
    {
        Task<AppointmentEditModel> PrepareAppointmentEditModelAsync(Appointment appointment);
        Task<AppointmentInfoModel> PrepareAppointmentInfoModel(Appointment appointment);
        Task<ProductCalendarModel> PrepareProductCalendarModel(ProductCalendarModel model, Product product);
        Task<VendorAppointmentInfoModel> PrepareVendorAppointmentInfoModel(Appointment appointment);
        Task<List<VendorResourceModel>> PrepareVendorResourcesModelAsync(int parentProductId);
    };
}
