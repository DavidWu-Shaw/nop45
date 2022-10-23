using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    public partial record CustomerSearchModel : BaseSearchModel
    {
        public bool IsLoggedInAsVendor { get; set; }
        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchMemberOfVendor")]
        public int SearchMemberOfVendorId { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }
    }
}
