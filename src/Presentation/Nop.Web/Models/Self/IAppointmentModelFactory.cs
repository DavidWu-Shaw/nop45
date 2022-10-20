using Nop.Core.Domain.Self;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Web.Models.Self
{
    public interface IAppointmentModelFactory
    {
        AppointmentUpdateModel PrepareAppointmentUpdateModel(Appointment appointment);
        AppointmentInfoModel PrepareAppointmentInfoModel(Appointment appointment);
        VendorAppointmentInfoModel PrepareVendorAppointmentInfoModel(Appointment appointment);
        Task<List<VendorResourceModel>> PrepareVendorResourcesModelAsync(int parentProductId);
    };
}