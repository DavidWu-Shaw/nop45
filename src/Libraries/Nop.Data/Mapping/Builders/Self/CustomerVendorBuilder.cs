using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Self;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.Self
{
    /// <summary>
    /// Represents a product picture entity builder
    /// </summary>
    public partial class CustomerVendorBuilder : NopEntityBuilder<CustomerVendor>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(CustomerVendor.CustomerId)).AsInt32().ForeignKey<Customer>()
                .WithColumn(nameof(CustomerVendor.VendorId)).AsInt32().ForeignKey<Vendor>();
        }

        #endregion
    }
}