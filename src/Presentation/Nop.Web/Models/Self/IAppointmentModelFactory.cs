using Nop.Core.Domain.Self;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Web.Models.Self
{
    public interface IAppointmentModelFactory
    {
        Task<AppointmentDetailModel> PrepareAppointmentDetailModel(Appointment appointment);
        Task<AppointmentInfoModel> PrepareAppointmentInfoModel(Appointment appointment);
        Task<VendorAppointmentInfoModel> PrepareVendorAppointmentInfoModel(Appointment appointment);
        Task<List<VendorResourceModel>> PrepareVendorResourcesModelAsync(int parentProductId);
    };
}