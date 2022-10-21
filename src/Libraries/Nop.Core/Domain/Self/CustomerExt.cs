using Nop.Core.Domain.Self;
using System.Collections.Generic;

namespace Nop.Core.Domain.Customers
{
    public partial class Customer
    {
        private ICollection<CustomerVendor> _customerVendorMappings;

        public void AddCustomerVendorMapping(CustomerVendor customerVendorMapping)
        {
            CustomerVendorMappings.Add(customerVendorMapping);
        }

        public void RemoveCustomerVendorMapping(CustomerVendor customerVendorMapping)
        {
            CustomerVendorMappings.Remove(customerVendorMapping);
        }

        public virtual ICollection<CustomerVendor> CustomerVendorMappings
        {
            get => _customerVendorMappings ?? (_customerVendorMappings = new List<CustomerVendor>());
            protected set => _customerVendorMappings = value;
        }
    }
}
