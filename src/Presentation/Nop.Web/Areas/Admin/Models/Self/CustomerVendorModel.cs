using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Self
{
    public record CustomerVendorModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }
        public int VendorId { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerVendors.Fields.VendorName")]
        public string VendorName { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerVendors.Fields.IsApproved")]
        public bool IsApproved { get; set; }
        [NopResourceDisplayName("Admin.Customers.CustomerVendors.Fields.IsFirstVendor")]
        public bool IsFirstVendor { get; set; }
    }
}
