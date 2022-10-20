using Nop.Core;
using Nop.Core.Domain.Customers;
using System;

namespace Nop.Services.Customers
{
    /// <summary>
    /// Customer service
    /// </summary>
    public partial class CustomerService : ICustomerService
    {
        /// <summary>
        /// Get customer instance without saving to database
        /// </summary>
        /// <param name="guid">Nullable Guid of customer Guid</param>
        /// <returns></returns>
        public Customer GetGuestCustomer(Guid? guid)
        {
            var customer = new Customer
            {
                CustomerGuid = guid.HasValue ? guid.Value : Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow
            };

            //add to 'Guests' role
            var guestRole = GetCustomerRoleBySystemName(NopCustomerDefaults.GuestsRoleName);
            if (guestRole == null)
                throw new NopException("'Guests' role could not be loaded");
            customer.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerRole = guestRole });

            return customer;
        }
    }
}
