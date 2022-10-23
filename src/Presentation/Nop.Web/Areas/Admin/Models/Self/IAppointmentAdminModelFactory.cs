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
        AppointmentInfoModel PrepareAppointmentInfoModel(Appointment appointment);
        ProductCalendarModel PrepareProductCalendarModel(ProductCalendarModel model, Product product);
        VendorAppointmentInfoModel PrepareVendorAppointmentInfoModel(Appointment appointment);
        Task<List<VendorResourceModel>> PrepareVendorResourcesModelAsync(int parentProductId);
    };
}
