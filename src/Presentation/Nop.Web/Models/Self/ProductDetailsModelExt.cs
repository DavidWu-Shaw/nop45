using Nop.Web.Framework.Models;
using Nop.Web.Models.Self;

namespace Nop.Web.Models.Catalog
{
    public partial record ProductDetailsModel : BaseNopEntityModel
    {
        public bool IsUserAuthenticated { get; set; }
        public string ManageCalendarUrl { get; set; }
        public bool IsUserAuthorizedToBookTime { get; set; } = true;

        public int BusinessBeginsHour { get; set; }
        public int BusinessEndsHour { get; set; }
        public int MaxHoursAllowed { get; set; }
        public int MaxFutureDays { get; set; }
        public int BusinessMorningShiftEndsHour { get; set; }
        public int BusinessAfternoonShiftBeginsHour { get; set; }
        public bool BusinessOnWeekends { get; set; }
    }
}
