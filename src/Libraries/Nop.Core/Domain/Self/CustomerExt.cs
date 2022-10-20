using Nop.Core.Domain.Self;
using System.Collections.Generic;

namespace Nop.Core.Domain.Customers
{
    public partial class Customer
    {
        private ICollection<CustomerVendorMapping> _customerVendorMappings;

        public void AddCustomerVendorMapping(CustomerVendorMapping customerVendorMapping)
        {
            CustomerVendorMappings.Add(customerVendorMapping);
        }

        public void RemoveCustomerVendorMapping(CustomerVendorMapping customerVendorMapping)
        {
            CustomerVendorMappings.Remove(customerVendorMapping);
        }

        public virtual ICollection<CustomerVendorMapping> CustomerVendorMappings
        {
            get => _customerVendorMappings ?? (_customerVendorMappings = new List<CustomerVendorMapping>());
            protected set => _customerVendorMappings = value;
        }
    }
}
