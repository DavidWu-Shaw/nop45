using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog
{
    public partial record ProductOverviewModel : BaseNopEntityModel
    {
        public bool IsUserAuthorizedToBookTime { get; set; }
    }
}
