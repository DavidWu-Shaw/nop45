using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Self
{
    public record AppointmentEditModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.Appointment.Fields.ResourceName")]
        public string ResourceName { get; set; }
        [NopResourceDisplayName("Admin.Appointment.Fields.TimeSlot")]
        public string TimeSlot { get; set; }
        [NopResourceDisplayName("Admin.Appointment.Fields.Status")]
        public string Status { get; set; }
        [NopResourceDisplayName("Admin.Appointment.Fields.Notes")]
        public string Notes { get; set; }
        public int ResourceId { get; set; }
        [NopResourceDisplayName("Admin.Appointment.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Admin.Appointment.Fields.CustomerEmail")]
        public string CustomerEmail { get; set; }
        [NopResourceDisplayName("Admin.Appointment.Fields.CustomerFullName")]
        public string CustomerFullName { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
    }
}
