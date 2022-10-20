using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;

namespace Nop.Core.Domain.Self
{
    /// <summary>
    /// Represents a product category mapping
    /// </summary>
    public partial class CustomerVendorMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the vendor identifier
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the vendor is the first one
        /// </summary>
        public bool IsFirstVendor { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the customer is approved as a member of the vendor
        /// </summary>
        public bool IsApproved { get; set; }
        
        /// <summary>
        /// Gets the customer
        /// </summary>
        public virtual Customer Customer { get; set; }

        /// <summary>
        /// Gets the vendor
        /// </summary>
        public virtual Vendor Vendor { get; set; }
    }
}
